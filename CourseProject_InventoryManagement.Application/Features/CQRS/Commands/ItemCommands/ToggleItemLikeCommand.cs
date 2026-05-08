using System;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.ItemCommands
{
    public class ToggleItemLikeCommand
    {
        public Guid ItemId { get; set; }
    }
}
