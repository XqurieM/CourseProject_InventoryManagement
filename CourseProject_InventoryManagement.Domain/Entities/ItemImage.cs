using CourseProject_InventoryManagement.Domain.Common;

namespace CourseProject_InventoryManagement.Domain.Entities
{
    public class ItemImage : AuditableEntity
    {
        public Guid ItemId { get; set; }
        public string ImageUrl { get; set; } = null!;
        public string? Caption { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPrimary { get; set; }
        public Item Item { get; set; } = null!;
    }
}
