using System;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.InventoryCommands
{
    public class DeleteOdooItemCommand
    {
        public string Token { get; set; } = null!;
        public string CustomId { get; set; } = null!;
    }
}
