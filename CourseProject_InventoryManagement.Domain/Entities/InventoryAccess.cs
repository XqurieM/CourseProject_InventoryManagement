using CourseProject_InventoryManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Domain.Entities
{
    public class InventoryAccess : AuditableEntity
    {
        public Guid InventoryId { get; set; }
        public Guid UserId { get; set; }
        public Inventory Inventory { get; set; } = null!;

    }
}
