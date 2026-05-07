namespace CourseProject_InventoryManagement.Application.Abstractions.Authorization
{
    public interface IReferenceDataAuthorizationService
    {
        Task<bool> CanReadCategoriesAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<bool> CanManageCategoriesAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<bool> CanReadTagsAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
