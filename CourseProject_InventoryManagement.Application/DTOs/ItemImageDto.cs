namespace CourseProject_InventoryManagement.Application.DTOs
{
    public class ItemImageDto
    {
        public Guid Id { get; set; }
        public Guid ItemId { get; set; }
        public string ImageUrl { get; set; } = null!;
        public string? Caption { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPrimary { get; set; }
    }
}
