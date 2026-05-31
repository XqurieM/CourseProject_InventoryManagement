using System;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.InventoryCommands
{
    public class GenerateInventoryApiTokenCommand
    {
        public Guid InventoryId { get; set; }
    }
}
