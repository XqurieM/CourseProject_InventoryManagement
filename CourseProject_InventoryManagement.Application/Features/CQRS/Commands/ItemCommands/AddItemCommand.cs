using CourseProject_InventoryManagement.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.ItemCommands
{
    public class AddItemCommand
    {
        public Guid InventoryId { get; set; }
        public List<ItemAddDto> Items { get; set; } = new();       
    }
}
