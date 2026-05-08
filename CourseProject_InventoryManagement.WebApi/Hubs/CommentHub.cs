using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.WebApi.Hubs
{
    public class CommentHub : Hub
    {
        // Clients can join a group specific to an inventory to receive updates only for that inventory
        public async Task JoinInventoryGroup(string inventoryId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Inventory_{inventoryId}");
        }

        public async Task LeaveInventoryGroup(string inventoryId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Inventory_{inventoryId}");
        }
    }
}
