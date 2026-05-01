using CourseProject_InventoryManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Results.InventoryResults
{
    public class GetInventoryFieldsByInventoryIdResult
    {
        public Guid Id { get; set; }
        public Guid InventoryId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public InventoryFieldType FieldType { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsRequired { get; set; }
        public bool ShowInTable { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public Guid UpdatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime DeletedAtUtc { get; set; }
    }
}
