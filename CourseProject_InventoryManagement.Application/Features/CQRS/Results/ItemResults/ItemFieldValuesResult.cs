using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Results.ItemResults
{
    public class ItemFieldValuesResult
    {
        public Guid Id { get; set; }
        public Guid ItemId { get; set; }
        public Guid InventoryFieldId { get; set; }
        public string StringValue { get; set; }
        public decimal NumberValue { get; set; }
        public bool BooleanValue { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public Guid UpdatedByUserId { get; set; }
    }
}
