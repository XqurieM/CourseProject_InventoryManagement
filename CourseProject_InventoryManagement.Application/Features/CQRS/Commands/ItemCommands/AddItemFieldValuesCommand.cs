using CourseProject_InventoryManagement.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.ItemCommands
{
    public class AddItemFieldValuesCommand
    {
        public Guid ItemId { get; set; }
        public List<ItemFieldValueDto> Values { get; set; } = new();
    }
}
