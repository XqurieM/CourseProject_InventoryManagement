using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Domain.Common
{
    public class BaseEntity
    {
        public Guid Id { get; protected set; }
    }
}
