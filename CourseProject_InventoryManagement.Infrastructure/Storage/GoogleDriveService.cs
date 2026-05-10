using CourseProject_InventoryManagement.Application.Abstractions.Storage;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Infrastructure.Keys;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using DriveFile = Google.Apis.Drive.v3.Data.File;
using DrivePermission = Google.Apis.Drive.v3.Data.Permission;

namespace CourseProject_InventoryManagement.Infrastructure.Storage
{
    public class GoogleDriveService : IStorageService
    {
        private static readonly string[] Scopes = [DriveService.Scope.DriveFile];

        private readonly GoogleDriveSettings _settings;
        private readonly IHostEnvironment _hostEnvironment;

        public GoogleDriveService(IOptions<GoogleDriveSettings> settings, IHostEnvironment hostEnvironment)
        {
            _settings = settings.Value;
            _hostEnvironment = hostEnvironment;
        }

        public async Task<UploadedFileResultDto> UploadFileAsync(IFormFile file, CancellationToken cancellationToken = default)
        {
            var service = GetDriveService();
            var uploadedFileName = $"{Guid.NewGuid():N}_{Path.GetFileName(file.FileName)}";

            var fileMetadata = new DriveFile
            {
                Name = uploadedFileName,
                Parents = new List<string> { _settings.FolderId }
            };

            await using var stream = file.OpenReadStream();
            var request = service.Files.Create(fileMetadata, stream, file.ContentType);
            request.Fields = "id";
            request.SupportsAllDrives = true;
            request.SupportsTeamDrives = true;
            request.IgnoreDefaultVisibility = true;

            var progress = await request.UploadAsync(cancellationToken);
            if (progress.Status == Google.Apis.Upload.UploadStatus.Failed)
            {
                throw CreateUploadException(progress.Exception);
            }

            var fileId = request.ResponseBody.Id;
            if (string.IsNullOrWhiteSpace(fileId))
            {
                throw new InvalidOperationException("Google Drive did not return a file id.");
            }

            await SetPublicPermissionAsync(service, fileId, cancellationToken);

            return new UploadedFileResultDto
            {
                FileName = uploadedFileName,
                RelativePath = fileId,
                Url = $"https://drive.google.com/uc?export=view&id={fileId}"
            };
        }

        private DriveService GetDriveService()
        {
            var fullPath = ResolveKeyFilePath();
            if (!System.IO.File.Exists(fullPath))
            {
                throw new FileNotFoundException($"JSON key file not found! Search path: {fullPath}");
            }

            GoogleCredential credential;
            using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
            }

            return new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = _settings.ApplicationName
            });
        }

        private async Task SetPublicPermissionAsync(DriveService service, string fileId, CancellationToken cancellationToken)
        {
            var permission = new DrivePermission
            {
                Type = "anyone",
                Role = "reader"
            };

            var permissionRequest = service.Permissions.Create(permission, fileId);
            permissionRequest.SupportsAllDrives = true;
            await permissionRequest.ExecuteAsync(cancellationToken);
        }

        private string ResolveKeyFilePath()
        {
            var configuredPath = _settings.KeyFilePath?.Trim();
            if (string.IsNullOrWhiteSpace(configuredPath))
            {
                throw new InvalidOperationException("Google Drive key file path is not configured.");
            }

            if (Path.IsPathRooted(configuredPath))
            {
                return configuredPath;
            }

            var candidatePaths = new[]
            {
                Path.GetFullPath(Path.Combine(_hostEnvironment.ContentRootPath, configuredPath)),
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, configuredPath)),
                Path.GetFullPath(Path.Combine(_hostEnvironment.ContentRootPath, "..", configuredPath))
            };

            return candidatePaths.FirstOrDefault(System.IO.File.Exists) ?? candidatePaths[0];
        }

        private InvalidOperationException CreateUploadException(Exception? exception)
        {
            var message = exception?.Message ?? "The upload failed for an unknown reason.";
            if (message.Contains("Service Accounts do not have storage quota", StringComparison.OrdinalIgnoreCase))
            {
                return new InvalidOperationException(
                    "Google Drive upload failed because the configured service account cannot upload into a personal My Drive folder. " +
                    "Move the target folder into a Shared Drive and share it with " +
                    "'inventorymanagementdriveapi@inventorymanagement-495118.iam.gserviceaccount.com' as Content manager, then keep using that folder id.");
            }

            return new InvalidOperationException($"Upload failed: {message}");
        }
    }
}
