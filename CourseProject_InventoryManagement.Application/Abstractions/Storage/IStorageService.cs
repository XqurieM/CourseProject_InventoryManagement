using CourseProject_InventoryManagement.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace CourseProject_InventoryManagement.Application.Abstractions.Storage
{
    public interface IStorageService
    {
        Task<UploadedFileResultDto> UploadFileAsync(IFormFile file, CancellationToken cancellationToken = default);
    }
}
