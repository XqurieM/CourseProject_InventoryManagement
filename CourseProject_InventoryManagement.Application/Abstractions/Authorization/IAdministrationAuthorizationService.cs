namespace CourseProject_InventoryManagement.Application.Abstractions.Authorization
{
    public interface IAdministrationAuthorizationService
    {
        Task<bool> CanManageUsersAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<bool> CanManageLocalizationAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
