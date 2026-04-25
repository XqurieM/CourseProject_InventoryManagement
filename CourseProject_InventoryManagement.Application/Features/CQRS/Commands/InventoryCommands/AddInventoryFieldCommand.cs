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
        public string Name { get; set; }
        public string? Description { get; set; }
        public InventoryFieldType FieldType { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsRequired { get; set; }
        public bool ShowInTable { get; set; }
        public Guid CreatedByUserId { get; set; }
    }
}
