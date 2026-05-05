using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Queries.InventoryQueries
{
    public class GetInventoryAccessListQuery
    {
        public Guid InventoryId { get; set; }
    }
}
