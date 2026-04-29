using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Domain.Entities;
using CourseProject_InventoryManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.InventoryCommands
{
    public class AddInventoryCustomIdRulesCommand
    {
        public Guid InventoryId { get; set; }
        public List<CustomIdRulePartDto> Rules { get; set; } = new();
    }
}
