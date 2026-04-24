using CourseProject_InventoryManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Domain.Entities
{
    public class ItemFieldValue : AuditableEntity
    {
        public Guid ItemId { get; set; }
        public Guid InventoryFieldId { get; set; }
        public string? StringValue { get; set; }
        public decimal? NumberValue { get; set; }
        public bool? BooleanValue { get; set; }
        public InventoryField InventoryField { get; set; } = null!;
        public Item Item { get; set; } = null!;
    }
}
