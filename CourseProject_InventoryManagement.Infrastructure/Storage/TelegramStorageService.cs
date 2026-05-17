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

        public TelegramStorageService(IOptions<TelegramStorageSettings> settings, HttpClient httpClient)
        {
            _settings = settings.Value;
            _httpClient = httpClient;
        }

        public async Task<UploadedFileResultDto> UploadFileAsync(IFormFile file, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File cannot be empty");

            if (string.IsNullOrWhiteSpace(_settings.BotToken) || string.IsNullOrWhiteSpace(_settings.ChatId))
                throw new InvalidOperationException("Telegram BotToken or ChatId is not configured.");

            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            var fileContent = new StreamContent(stream);
            if (!string.IsNullOrEmpty(file.ContentType))
            {
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);
            }
            
            content.Add(new StringContent(_settings.ChatId), "chat_id");
            content.Add(fileContent, "photo", file.FileName);

            var url = $"https://api.telegram.org/bot{_settings.BotToken}/sendPhoto";
            var response = await _httpClient.PostAsync(url, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(responseString);
            

            var photoArray = document.RootElement
                .GetProperty("result")
                .GetProperty("photo");
                
            var fileId = photoArray[photoArray.GetArrayLength() - 1]
                .GetProperty("file_id")
                .GetString();

            if (string.IsNullOrEmpty(fileId))
                throw new Exception("Failed to retrieve file_id from Telegram");

            var proxyUrl = $"/General/ProxyTelegramImage?fileId={fileId}";

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
    }
}
