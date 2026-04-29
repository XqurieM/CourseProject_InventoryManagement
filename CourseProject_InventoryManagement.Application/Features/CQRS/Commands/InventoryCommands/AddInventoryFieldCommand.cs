using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.InventoryCommands
{
    public class AddInventoryFieldCommand
    {
        public Guid InventoryId { get; set; }
        public List<InventoryFieldDto> Fields { get; set; } = new();
    }
}
