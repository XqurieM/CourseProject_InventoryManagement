using Ardalis.Result;
using CourseProject_InventoryManagement.Domain.Entities;

namespace CourseProject_InventoryManagement.Application.Abstractions.Authentication
{
    public interface IAuthenticatedUserService
    {
        Task<Result<AppUser>> GetRequiredUserAsync(CancellationToken cancellationToken = default);
        Task<Result<AppUser>> GetRequiredAdminAsync(CancellationToken cancellationToken = default);
    }
}
