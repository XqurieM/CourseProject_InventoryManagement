using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Integrations;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.UserCommands;
using CourseProject_InventoryManagement.Infrastructure.Keys;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CourseProject_InventoryManagement.Infrastructure.Integrations
{
    public class SalesforceService : ISalesforceService
    {
        private readonly SalesforceSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly ILogger<SalesforceService> _logger;

        public SalesforceService(
            IOptions<SalesforceSettings> settings,
            HttpClient httpClient,
            ILogger<SalesforceService> logger)
        {
            _settings = settings.Value;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<Result<SalesforceIntegrationResultDto>> IntegrateUserAsync(
            IntegrateSalesforceCommand command,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Salesforce Client Credentials Integration triggered for User: {UserId}, AccountName: {AccountName}, ContactEmail: {Email}",
                command.UserId, command.AccountName, command.Email);

            var authResult = await AuthenticateAsync(cancellationToken);
            if (!authResult.IsSuccess)
            {
                var errorMsg = string.Join(" | ", authResult.Errors);
                _logger.LogError("Salesforce Authentication failed: {Error}", errorMsg);
                return Result<SalesforceIntegrationResultDto>.Error($"Salesforce Authentication failed: {errorMsg}");
            }

            var accessToken = authResult.Value.AccessToken;
            var instanceUrl = authResult.Value.InstanceUrl.TrimEnd('/');

            _logger.LogInformation("Salesforce Authentication successful. Instance URL: {InstanceUrl}", instanceUrl);

            var accountResult = await CreateAccountAsync(instanceUrl, accessToken, command, cancellationToken);
            if (!accountResult.IsSuccess)
            {
                var errorMsg = string.Join(" | ", accountResult.Errors);
                _logger.LogError("Salesforce Account creation failed: {Error}", errorMsg);
                return Result<SalesforceIntegrationResultDto>.Error($"Salesforce Account creation failed: {errorMsg}");
            }

            var accountId = accountResult.Value;
            _logger.LogInformation("Salesforce Account created successfully. AccountId: {AccountId}", accountId);

            var contactResult = await CreateContactAsync(instanceUrl, accessToken, accountId, command, cancellationToken);
            if (!contactResult.IsSuccess)
            {
                var errorMsg = string.Join(" | ", contactResult.Errors);
                _logger.LogError("Salesforce Contact creation failed: {Error}", errorMsg);
                return Result<SalesforceIntegrationResultDto>.Error($"Salesforce Contact creation failed: {errorMsg}");
            }

            var contactId = contactResult.Value;
            _logger.LogInformation("Salesforce Contact created successfully. ContactId: {ContactId} linked to AccountId: {AccountId}", contactId, accountId);

            var resultDto = new SalesforceIntegrationResultDto
            {
                AccountId = accountId,
                ContactId = contactId,
                Message = "User data successfully integrated with Salesforce CRM."
            };

            return Result<SalesforceIntegrationResultDto>.Success(resultDto);
        }

        private async Task<Result<(string AccessToken, string InstanceUrl)>> AuthenticateAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_settings.ClientId) ||
                string.IsNullOrWhiteSpace(_settings.ClientSecret))
            {
                return Result<(string, string)>.Error("Salesforce API configuration settings (ClientId, ClientSecret) are incomplete in appsettings.json.");
            }

            var authParams = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", _settings.ClientId },
                { "client_secret", _settings.ClientSecret }
            };

            using var content = new FormUrlEncodedContent(authParams);
            
            var requestUrl = !string.IsNullOrWhiteSpace(_settings.AuthUrl) 
                ? _settings.AuthUrl.TrimEnd('/') + "/services/oauth2/token"
                : "https://login.salesforce.com/services/oauth2/token";

            try
            {
                var response = await _httpClient.PostAsync(requestUrl, content, cancellationToken);
                var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return Result<(string, string)>.Error($"Salesforce token endpoint returned status {(int)response.StatusCode}: {responseString}");
                }

                using var jsonDoc = JsonDocument.Parse(responseString);
                var root = jsonDoc.RootElement;

                if (root.TryGetProperty("access_token", out var tokenElement) &&
                    root.TryGetProperty("instance_url", out var urlElement))
                {
                    var accessToken = tokenElement.GetString();
                    var instanceUrl = urlElement.GetString();

                    if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(instanceUrl))
                    {
                        return Result<(string, string)>.Success((accessToken, instanceUrl));
                    }
                }

                return Result<(string, string)>.Error("Failed to parse access_token or instance_url from Salesforce Client Credentials response.");
            }
            catch (Exception ex)
            {
                return Result<(string, string)>.Error($"An exception occurred during Salesforce Client Credentials request: {ex.Message}");
            }
        }

        private async Task<Result<string>> CreateAccountAsync(
            string instanceUrl,
            string accessToken,
            IntegrateSalesforceCommand command,
            CancellationToken cancellationToken)
        {
            var url = $"{instanceUrl}/services/data/v60.0/sobjects/Account";

            var accountData = new Dictionary<string, object>();
            accountData.Add("Name", command.AccountName);

            if (!string.IsNullOrWhiteSpace(command.AccountPhone))
                accountData.Add("Phone", command.AccountPhone);
            if (!string.IsNullOrWhiteSpace(command.BillingStreet))
                accountData.Add("BillingStreet", command.BillingStreet);
            if (!string.IsNullOrWhiteSpace(command.BillingCity))
                accountData.Add("BillingCity", command.BillingCity);
            //if (!string.IsNullOrWhiteSpace(command.BillingState))
            //    accountData.Add("BillingState", command.BillingState);
            if (!string.IsNullOrWhiteSpace(command.BillingPostalCode))
                accountData.Add("BillingPostalCode", command.BillingPostalCode);
            if (!string.IsNullOrWhiteSpace(command.BillingCountry))
                accountData.Add("BillingCountryCode", command.BillingCountry);

            var json = JsonSerializer.Serialize(accountData);
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.SendAsync(request, cancellationToken);
                var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return Result<string>.Error($"Salesforce Account creation endpoint returned status {(int)response.StatusCode}: {responseString}");
                }

                using var jsonDoc = JsonDocument.Parse(responseString);
                var root = jsonDoc.RootElement;

                if (root.TryGetProperty("success", out var successElement) && successElement.GetBoolean() &&
                    root.TryGetProperty("id", out var idElement))
                {
                    var accountId = idElement.GetString();
                    if (!string.IsNullOrEmpty(accountId))
                    {
                        return Result<string>.Success(accountId);
                    }
                }

                return Result<string>.Error("Salesforce Account creation response was marked as failure or missing Account Id.");
            }
            catch (Exception ex)
            {
                return Result<string>.Error($"An exception occurred during Salesforce Account creation request: {ex.Message}");
            }
        }

        private async Task<Result<string>> CreateContactAsync(
            string instanceUrl,
            string accessToken,
            string accountId,
            IntegrateSalesforceCommand command,
            CancellationToken cancellationToken)
        {
            var url = $"{instanceUrl}/services/data/v60.0/sobjects/Contact";

            var contactData = new Dictionary<string, object>();
            contactData.Add("FirstName", command.FirstName);
            contactData.Add("LastName", command.LastName);
            contactData.Add("Email", command.Email);
            contactData.Add("AccountId", accountId);

            if (!string.IsNullOrWhiteSpace(command.ContactPhone))
                contactData.Add("Phone", command.ContactPhone);
            if (!string.IsNullOrWhiteSpace(command.JobTitle))
                contactData.Add("Title", command.JobTitle);
            if (!string.IsNullOrWhiteSpace(command.Department))
                contactData.Add("Department", command.Department);

            var json = JsonSerializer.Serialize(contactData);
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.SendAsync(request, cancellationToken);
                var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return Result<string>.Error($"Salesforce Contact creation endpoint returned status {(int)response.StatusCode}: {responseString}");
                }

                using var jsonDoc = JsonDocument.Parse(responseString);
                var root = jsonDoc.RootElement;

                if (root.TryGetProperty("success", out var successElement) && successElement.GetBoolean() &&
                    root.TryGetProperty("id", out var idElement))
                {
                    var contactId = idElement.GetString();
                    if (!string.IsNullOrEmpty(contactId))
                    {
                        return Result<string>.Success(contactId);
                    }
                }

                return Result<string>.Error("Salesforce Contact creation response was marked as failure or missing Contact Id.");
            }
            catch (Exception ex)
            {
                return Result<string>.Error($"An exception occurred during Salesforce Contact creation request: {ex.Message}");
            }
        }
    }
}