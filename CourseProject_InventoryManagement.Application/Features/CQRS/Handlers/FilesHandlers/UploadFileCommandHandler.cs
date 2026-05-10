using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Storage;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.FilesCommands;
using Microsoft.AspNetCore.Http;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.FilesHandlers
{
    public class UploadFileCommandHandler : ICQRS.IUploadFile
    {
        private readonly IStorageService _storageService;

        public UploadFileCommandHandler(IStorageService storageService)
        {
            _storageService = storageService;
        }

        public async Task<Result<UploadedFileResultDto>> UploadFile(UploadFileCommand command, CancellationToken cancellationToken = default)
        {
            if (command.File is null || command.File.Length == 0)
            {
                return Result<UploadedFileResultDto>.Invalid(new ValidationError(nameof(command.File), "A file is required."));
            }

            if (!command.File.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return Result<UploadedFileResultDto>.Invalid(new ValidationError(nameof(command.File), "Only image uploads are allowed."));
            }

            try
            {
                var uploadResult = await _storageService.UploadFileAsync(command.File, cancellationToken);
                return Result<UploadedFileResultDto>.Success(uploadResult);
            }
            catch (FileNotFoundException ex)
            {
                return Result<UploadedFileResultDto>.Error(ex.Message);
            }
            catch (Exception ex)
            {
                return Result<UploadedFileResultDto>.Error($"File upload failed: {ex.Message}");
            }
        }
    }
}
