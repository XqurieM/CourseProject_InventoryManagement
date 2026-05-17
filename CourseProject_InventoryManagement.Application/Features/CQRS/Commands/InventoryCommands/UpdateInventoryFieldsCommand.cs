using CourseProject_InventoryManagement.Application.DTOs;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.InventoryCommands
{
    public class UpdateInventoryFieldsCommand
    {
        public Guid InventoryId { get; set; }
        public List<UpdateInventoryFieldDto> Fields { get; set; } = new();
    }
}
