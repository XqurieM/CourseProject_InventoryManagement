using CourseProject_InventoryManagement.Application.Abstractions.Notifications;
using CourseProject_InventoryManagement.WebApi.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.WebApi.Services
{
    public class CommentNotificationService : ICommentNotificationService
    {
        private readonly IHubContext<CommentHub> _hubContext;

        public CommentNotificationService(IHubContext<CommentHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendCommentAddedNotificationAsync(Guid inventoryId, Guid commentId, string content, Guid userId, string userName, DateTime createdAtUtc, CancellationToken cancellationToken = default)
        {
            var groupName = $"Inventory_{inventoryId}";
            var payload = new
            {
                CommentId = commentId,
                InventoryId = inventoryId,
                Content = content,
                UserId = userId,
                UserName = userName,
                CreatedAtUtc = createdAtUtc
            };

            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNewComment", payload, cancellationToken);
        }
    }
}
