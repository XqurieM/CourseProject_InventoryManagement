using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Domain.Common
{
    public class AuditableEntity
    {
        public DateTime CreatedAtUtc { get; protected set; }
        public Guid CreatedByUserId { get; protected set; }
        public DateTime? UpdatedAtUtc { get; protected set; }
        public Guid? UpdatedByUserId { get; protected set; }
    }
}
