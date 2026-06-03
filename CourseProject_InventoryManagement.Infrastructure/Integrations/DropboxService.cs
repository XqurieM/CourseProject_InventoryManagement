using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Integrations;
using CourseProject_InventoryManagement.Infrastructure.Keys;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CourseProject_InventoryManagement.Infrastructure.Integrations
{
    public class DropboxService : IDropboxService
    {
        private readonly DropboxSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly ILogger<DropboxService> _logger;

        public DropboxService(
            IOptions<DropboxSettings> settings,
            HttpClient httpClient,
            ILogger<DropboxService> logger)
        {
            _settings = settings.Value;
            _httpClient = httpClient;
            _logger = logger;
        }

        private async Task<string> GetValidAccessTokenAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_settings.AppKey) || 
                string.IsNullOrWhiteSpace(_settings.AppSecret) || 
                string.IsNullOrWhiteSpace(_settings.RefreshToken))
            {
                return _settings.AccessToken;
            }

            try
            {
                _logger.LogInformation("Refreshing Dropbox Access Token using Refresh Token...");
                using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.dropboxapi.com/oauth2/token");
                var parameters = new Dictionary<string, string>
                {
                    { "grant_type", "refresh_token" },
                    { "refresh_token", _settings.RefreshToken }
                };

                var authenticationString = $"{_settings.AppKey}:{_settings.AppSecret}";
                var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(authenticationString));

                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
                request.Content = new FormUrlEncodedContent(parameters);

                var response = await _httpClient.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var errorStr = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning("Failed to refresh Dropbox AccessToken. Status: {Status}, Details: {Details}", response.StatusCode, errorStr);
                    return _settings.AccessToken;
                }

                var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(responseString);
                if (doc.RootElement.TryGetProperty("access_token", out var tokenProp))
                {
                    var refreshedToken = tokenProp.GetString();
                    if (!string.IsNullOrEmpty(refreshedToken))
                    {
                        _logger.LogInformation("Dropbox Access Token refreshed successfully.");
                        return refreshedToken;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while refreshing Dropbox Access Token.");
            }

            return _settings.AccessToken;
        }

        public async Task<Result<string>> UploadFileAsync(
            string fileName,
            string fileContent,
            CancellationToken cancellationToken = default)
        {
            var accessToken = await GetValidAccessTokenAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                _logger.LogWarning("Dropbox AccessToken is not configured in appsettings.json. Support ticket JSON will only be logged locally.");
                return Result<string>.Error("Dropbox AccessToken is not configured in appsettings.json.");
            }

            var requestUrl = "https://content.dropboxapi.com/2/files/upload";
            var path = $"{_settings.FolderPath.TrimEnd('/')}/{fileName}";

            using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            
            var apiArg = new
            {
                path = path,
                mode = "overwrite",
                autorename = true,
                mute = false,
                strict_conflict = false
            };

            var argJson = JsonSerializer.Serialize(apiArg);
            request.Headers.Add("Dropbox-API-Arg", argJson);

            
            var content = new ByteArrayContent(Encoding.UTF8.GetBytes(fileContent));
            content.Headers.Remove("Content-Type");
            content.Headers.TryAddWithoutValidation("Content-Type", "application/octet-stream");
            request.Content = content;

            try
            {
                _logger.LogInformation("Uploading ticket JSON to Dropbox path: {Path}", path);
                var response = await _httpClient.SendAsync(request, cancellationToken);
                var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Dropbox upload failed with status code {StatusCode}: {Response}", (int)response.StatusCode, responseString);
                    return Result<string>.Error($"Dropbox API returned status {(int)response.StatusCode}: {responseString}");
                }

                _logger.LogInformation("Successfully uploaded support ticket {FileName} to Dropbox.", fileName);
                return Result<string>.Success(path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while uploading support ticket {FileName} to Dropbox", fileName);
                return Result<string>.Error($"Dropbox upload exception: {ex.Message}");
            }
        }
    }
}
