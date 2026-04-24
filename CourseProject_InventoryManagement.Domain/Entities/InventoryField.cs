using CourseProject_InventoryManagement.Domain.Common;
using CourseProject_InventoryManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Domain.Entities
{
    public class InventoryField : SoftDeletableEntity
    {
        public Guid InventoryId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public InventoryFieldType FieldType { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsRequired { get; set; }
        public bool ShowInTable { get; set; }
        public Inventory Inventory { get; set; } = null!;
        public ICollection<ItemFieldValue> ItemFieldValues { get; set; } = new List<ItemFieldValue>();
    }
}
