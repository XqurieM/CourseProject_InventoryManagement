using CourseProject_InventoryManagement.Application.DTOs;
using System.Collections.Generic;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.InventoryCommands
{
    public class CreateOdooItemsCommand
    {
        public string Token { get; set; } = null!;
        public List<OdooCreateItemDto> Items { get; set; } = new();
    }
}
