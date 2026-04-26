using CourseProject_InventoryManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.DTOs
{
    public class CustomIdRulePartDto
    {
        public int PartOrder { get; set; }
        public CustomIdPartType PartType { get; set; }
        public string? Format { get; set; }
        public string? StaticTextValue { get; set; }
    }
}
