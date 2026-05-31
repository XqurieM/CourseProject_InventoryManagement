using System;
using System.Collections.Generic;

namespace CourseProject_InventoryManagement.Application.DTOs
{
    public class OdooInventoryAggregateDto
    {
        public Guid InventoryId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int TotalItems { get; set; }
        public string? CategoryName { get; set; }
        public List<string> Tags { get; set; } = new();
        public int CommentCount { get; set; }
        public int FieldCount { get; set; }
        public int LikeCount { get; set; }
        public int ExplicitAccessCount { get; set; }
        public List<OdooFieldAggregateDto> Fields { get; set; } = new();
        public List<OdooItemDto> Items { get; set; } = new();
    }

    public class OdooItemDto
    {
        public Guid ItemId { get; set; }
        public string ItemName { get; set; } = null!;
        public string CustomId { get; set; } = null!;
        public int LikeCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<OdooItemFieldValueDto> FieldValues { get; set; } = new();
    }

    public class OdooItemFieldValueDto
    {
        public string FieldName { get; set; } = null!;
        public string Value { get; set; } = null!;
        public string FieldType { get; set; } = null!;
    }

    public class OdooFieldAggregateDto
    {
        public Guid FieldId { get; set; }
        public string Name { get; set; } = null!;
        public string FieldType { get; set; } = null!;         
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public decimal? AverageValue { get; set; }
        public List<OdooStringValueCountDto>? MostPopularValues { get; set; }
        public int? TrueCount { get; set; }
        public int? FalseCount { get; set; }
    }

    public class OdooStringValueCountDto
    {
        public string Value { get; set; } = null!;
        public int Count { get; set; }
    }
}
