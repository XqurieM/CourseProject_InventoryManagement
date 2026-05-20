using CourseProject_InventoryManagement.Application.DTOs;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.ItemCommands
{
    public class UpdateItemImagesCommand
    {
        public Guid ItemId { get; set; }
        public List<UpdateItemImageDto> Images { get; set; } = new();
    }
}
