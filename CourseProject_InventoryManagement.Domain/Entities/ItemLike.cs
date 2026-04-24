using CourseProject_InventoryManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Domain.Entities
{
    public class ItemLike : AuditableEntity
    {
        public Guid ItemId { get; set; }
        public Item Item { get; set; } = null!;
    }
}
