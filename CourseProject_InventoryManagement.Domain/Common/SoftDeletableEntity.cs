using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Domain.Common
{
    public abstract class SoftDeletableEntity : AuditableEntity
    {
        public bool IsDeleted { get; protected set; }

        public DateTime? DeletedAtUtc { get; protected set; }
    }
}
