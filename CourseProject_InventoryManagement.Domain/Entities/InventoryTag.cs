using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Domain.Entities
{
    public class InventoryTag
    {
        public Guid InventoryId { get; set; }
        public Guid TagId { get; set; }
        public Inventory Inventory { get; set; } = null!;
        public Tag Tag { get; set; } = null!;
    }
}
