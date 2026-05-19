using System;
using System.Threading;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Abstractions.Notifications
{
    public interface ICommentNotificationService
    {
        Task SendCommentAddedNotificationAsync(Guid inventoryId, Guid commentId, string content, Guid userId, string userName, DateTime createdAtUtc, CancellationToken cancellationToken = default);
        Task SendCommentUpdatedNotificationAsync(Guid inventoryId, Guid commentId, string content, Guid userId, string userName, DateTime updatedAtUtc, CancellationToken cancellationToken = default);
        Task SendCommentDeletedNotificationAsync(Guid inventoryId, Guid commentId, CancellationToken cancellationToken = default);
    }
}
