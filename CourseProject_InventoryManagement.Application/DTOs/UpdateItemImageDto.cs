namespace CourseProject_InventoryManagement.Application.DTOs
{
    public class UpdateItemImageDto
    {
        public string ImageUrl { get; set; } = null!;
        public string? Caption { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPrimary { get; set; }
    }
}
