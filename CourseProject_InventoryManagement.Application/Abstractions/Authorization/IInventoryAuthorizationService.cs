namespace CourseProject_InventoryManagement.Application.Abstractions.Authorization
{
    public interface IInventoryAuthorizationService
    {
        Task<bool> CanManageInventoryAsync(Guid inventoryId, Guid userId, CancellationToken cancellationToken = default);
        Task<bool> CanWriteItemsAsync(Guid inventoryId, Guid userId, CancellationToken cancellationToken = default);
    }
}
