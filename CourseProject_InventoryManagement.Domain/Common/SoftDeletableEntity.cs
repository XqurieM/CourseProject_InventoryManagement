using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Domain.Common
{
    public class SoftDeletableEntity
    {
        public bool IsDeleted { get; protected set; }

        public DateTime? DeletedAtUtc { get; protected set; }
    }
}
