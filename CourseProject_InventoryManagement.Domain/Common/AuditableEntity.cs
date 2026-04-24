using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Domain.Common
{
    public abstract class AuditableEntity : BaseEntity
    {
        public DateTime CreatedAtUtc { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public Guid? UpdatedByUserId { get; set; }
    }
}
