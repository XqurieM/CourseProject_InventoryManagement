namespace CourseProject_InventoryManagement.Application.Abstractions.Authorization
{
    public interface IInventoryAuthorizationService
    {
        Task<bool> CanViewInventoryAsync(Guid inventoryId, Guid? userId, CancellationToken cancellationToken = default);
        Task<bool> CanManageInventoryAsync(Guid inventoryId, Guid userId, CancellationToken cancellationToken = default);
        Task<bool> CanManageInventoryFieldsAsync(Guid inventoryId, Guid userId, CancellationToken cancellationToken = default);
        Task<bool> CanManageInventoryCustomIdRulesAsync(Guid inventoryId, Guid userId, CancellationToken cancellationToken = default);
        Task<bool> CanManageInventoryAccessAsync(Guid inventoryId, Guid userId, CancellationToken cancellationToken = default);
        Task<bool> CanViewInventoryAccessListAsync(Guid inventoryId, Guid userId, CancellationToken cancellationToken = default);
        Task<bool> CanWriteItemsAsync(Guid inventoryId, Guid userId, CancellationToken cancellationToken = default);
        Task<bool> CanCreateItemsAsync(Guid inventoryId, Guid userId, CancellationToken cancellationToken = default);
        Task<bool> CanUpdateItemsAsync(Guid inventoryId, Guid userId, CancellationToken cancellationToken = default);
        Task<bool> CanDeleteItemsAsync(Guid inventoryId, Guid userId, CancellationToken cancellationToken = default);
        Task<bool> CanCommentOnInventoryAsync(Guid inventoryId, Guid userId, CancellationToken cancellationToken = default);
    }
}
