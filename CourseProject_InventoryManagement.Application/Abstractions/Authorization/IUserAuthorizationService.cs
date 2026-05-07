namespace CourseProject_InventoryManagement.Application.Abstractions.Authorization
{
    public interface IUserAuthorizationService
    {
        Task<bool> CanAccessUserScopedDataAsync(Guid requestedUserId, Guid currentUserId, CancellationToken cancellationToken = default);
    }
}
