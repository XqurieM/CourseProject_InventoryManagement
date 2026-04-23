using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Domain.Common
{
    public class AuditableEntity
    {
        public DateTime CreatedAtUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public int UpdatedByUserId { get; set; }
    }
}
