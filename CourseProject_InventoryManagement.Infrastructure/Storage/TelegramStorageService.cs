using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Storage;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Infrastructure.Keys;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CourseProject_InventoryManagement.Infrastructure.Storage
{
    public class TelegramStorageService : IStorageService, ITelegramStorageProxy
    {
        private readonly TelegramStorageSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TelegramStorageService(IOptions<TelegramStorageSettings> settings, HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _settings = settings.Value;
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<UploadedFileResultDto> UploadFileAsync(IFormFile file, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File cannot be empty");

            if (string.IsNullOrWhiteSpace(_settings.BotToken) || string.IsNullOrWhiteSpace(_settings.ChatId))
                throw new InvalidOperationException("Telegram BotToken or ChatId is not configured.");

            var fileId = await UploadToTelegramAndGetFileIdAsync(file, cancellationToken);

            if (string.IsNullOrEmpty(fileId))
                throw new Exception("Failed to retrieve file_id from Telegram");

            var request = _httpContextAccessor.HttpContext?.Request;
            var baseUrl = NormalizeBaseUrl(
                !string.IsNullOrWhiteSpace(_settings.ApiBaseUrl)
                    ? _settings.ApiBaseUrl
                    : (request != null ? $"{request.Scheme}://{request.Host}" : ""));
            
            var proxyUrl = $"{baseUrl.TrimEnd('/')}/General/ProxyTelegramImage?fileId={fileId}";

            return new UploadedFileResultDto
            {
                FileName = file.FileName,
                RelativePath = fileId,
                Url = proxyUrl
            };
        }

        public async Task<Result<string>> GetFileDirectUrlAsync(string fileId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_settings.BotToken))
                return Result<string>.Error("Telegram BotToken is not configured.");

            var url = $"https://api.telegram.org/bot{_settings.BotToken}/getFile?file_id={fileId}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
                return Result<string>.NotFound("File not found on Telegram.");

            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(responseString);
            
            var isOk = document.RootElement.GetProperty("ok").GetBoolean();
            if (!isOk)
                return Result<string>.Error("Telegram API returned an error.");

            var filePath = document.RootElement
                .GetProperty("result")
                .GetProperty("file_path")
                .GetString();

            if (string.IsNullOrEmpty(filePath))
                return Result<string>.Error("Failed to parse file_path from Telegram.");

            var directUrl = $"https://api.telegram.org/file/bot{_settings.BotToken}/{filePath}";
            return Result<string>.Success(directUrl);
        }

        private async Task<string> UploadToTelegramAndGetFileIdAsync(IFormFile file, CancellationToken cancellationToken)
        {
            var photoResult = await TryUploadAsync(file, "sendPhoto", "photo", cancellationToken);
            if (photoResult.IsSuccess)
            {
                return photoResult.Value;
            }

            var documentResult = await TryUploadAsync(file, "sendDocument", "document", cancellationToken);
            if (documentResult.IsSuccess)
            {
                return documentResult.Value;
            }

            throw new HttpRequestException(
                $"Telegram upload failed. Photo error: {GetResultMessage(photoResult)}. Document error: {GetResultMessage(documentResult)}.");
        }

        private async Task<Result<string>> TryUploadAsync(IFormFile file, string endpoint, string fileFieldName, CancellationToken cancellationToken)
        {
            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            using var fileContent = new StreamContent(stream);

            if (!string.IsNullOrEmpty(file.ContentType))
            {
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);
            }

            content.Add(new StringContent(_settings.ChatId), "chat_id");
            content.Add(fileContent, fileFieldName, file.FileName);

            var url = $"https://api.telegram.org/bot{_settings.BotToken}/{endpoint}";
            var response = await _httpClient.PostAsync(url, content, cancellationToken);
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Result<string>.Error($"Telegram {endpoint} failed with {(int)response.StatusCode}: {responseString}");
            }

            using var document = JsonDocument.Parse(responseString);
            if (!document.RootElement.TryGetProperty("ok", out var okElement) || !okElement.GetBoolean())
            {
                return Result<string>.Error($"Telegram {endpoint} returned a failure payload.");
            }

            if (!document.RootElement.TryGetProperty("result", out var resultElement))
            {
                return Result<string>.Error($"Telegram {endpoint} response did not include a result payload.");
            }

            if (endpoint.Equals("sendPhoto", StringComparison.OrdinalIgnoreCase))
            {
                if (resultElement.TryGetProperty("photo", out var photoArray) && photoArray.ValueKind == JsonValueKind.Array && photoArray.GetArrayLength() > 0)
                {
                    var fileId = photoArray[photoArray.GetArrayLength() - 1].GetProperty("file_id").GetString();
                    if (!string.IsNullOrWhiteSpace(fileId))
                    {
                        return Result<string>.Success(fileId);
                    }
                }
            }
            else if (resultElement.TryGetProperty("document", out var documentElement))
            {
                var fileId = documentElement.GetProperty("file_id").GetString();
                if (!string.IsNullOrWhiteSpace(fileId))
                {
                    return Result<string>.Success(fileId);
                }
            }

            return Result<string>.Error($"Telegram {endpoint} response did not contain a usable file_id.");
        }

        private static string GetResultMessage(Result<string> result)
        {
            if (result.Errors.Any())
            {
                return string.Join(" | ", result.Errors);
            }

            if (result.ValidationErrors.Any())
            {
                return string.Join(" | ", result.ValidationErrors.Select(x => x.ErrorMessage));
            }

            return result.Status.ToString();
        }

        private static string NormalizeBaseUrl(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return string.Empty;
            }

            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
            {
                return baseUrl;
            }

            if (uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) && !uri.IsLoopback)
            {
                var builder = new UriBuilder(uri)
                {
                    Scheme = Uri.UriSchemeHttps,
                    Port = uri.Port == 80 ? -1 : uri.Port
                };

                return builder.Uri.ToString().TrimEnd('/');
            }

            return uri.ToString().TrimEnd('/');
        }
    }
}
