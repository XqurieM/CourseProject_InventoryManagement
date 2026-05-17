namespace CourseProject_InventoryManagement.Application.DTOs
{
    public class InventoryAccessUserLookupDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public bool IsAlreadyAdded { get; set; }
    }
}
