using CourseProject_InventoryManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Domain.Entities
{
    public class Tag : BaseEntity
    {
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public ICollection<InventoryTag> InventoryTags { get; set; } = new List<InventoryTag>();
    }
}
