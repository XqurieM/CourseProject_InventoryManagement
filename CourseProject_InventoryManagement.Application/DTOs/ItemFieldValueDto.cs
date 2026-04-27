using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.DTOs
{
    public class ItemFieldValueDto
    {
        public Guid InventoryFieldId { get; set; }
        public string? StringValue { get; set; }
        public decimal? NumberValue { get; set; }
        public bool? BooleanValue { get; set; }
    }
}
