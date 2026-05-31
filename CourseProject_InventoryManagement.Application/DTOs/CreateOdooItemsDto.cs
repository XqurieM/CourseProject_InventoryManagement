using System.Collections.Generic;

namespace CourseProject_InventoryManagement.Application.DTOs
{
    public class OdooCreateFieldValueDto
    {
        public string FieldName { get; set; } = null!;
        public string Value { get; set; } = null!;
    }

    public class OdooCreateItemDto
    {
        public string ItemName { get; set; } = null!;
        public string? CustomId { get; set; }
        public List<OdooCreateFieldValueDto> FieldValues { get; set; } = new();
    }
}
