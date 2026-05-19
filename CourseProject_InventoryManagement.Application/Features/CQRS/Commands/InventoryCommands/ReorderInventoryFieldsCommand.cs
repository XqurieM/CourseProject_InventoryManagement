using CourseProject_InventoryManagement.Application.DTOs;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.InventoryCommands
{
    public class ReorderInventoryFieldsCommand
    {
        public Guid InventoryId { get; set; }
        public List<ReorderInventoryFieldDto> Fields { get; set; } = new();
    }
}
