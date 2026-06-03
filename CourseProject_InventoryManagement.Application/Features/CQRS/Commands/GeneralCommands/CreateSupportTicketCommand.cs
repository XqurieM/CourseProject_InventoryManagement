using System;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.GeneralCommands
{
    public class CreateSupportTicketCommand
    {
        public string Summary { get; set; } = null!;
        public string Priority { get; set; } = "Average"; 
        public Guid? InventoryId { get; set; }
        public string Link { get; set; } = null!;
    }
}
