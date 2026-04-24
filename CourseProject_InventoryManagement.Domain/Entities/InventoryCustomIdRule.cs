using CourseProject_InventoryManagement.Domain.Common;
using CourseProject_InventoryManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Domain.Entities
{
    public class InventoryCustomIdRule : AuditableEntity
    {
        public Guid InventoryId { get; set; }
        public int PartOrder { get; set; }
        public CustomIdPartType PartType { get; set; }
        public string? Format { get; set; }
        public string? StaticTextValue { get; set; }
        public Inventory Inventory { get; set; } = null!;
    }
}
