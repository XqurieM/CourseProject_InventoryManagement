using CourseProject_InventoryManagement.Application.DTOs;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.ItemCommands
{
    public class UpdateItemFieldValuesCommand
    {
        public Guid ItemId { get; set; }
        public List<ItemFieldValueDto> Values { get; set; } = new();
    }
}
