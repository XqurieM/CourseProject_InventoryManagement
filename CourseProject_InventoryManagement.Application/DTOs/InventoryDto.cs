using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.DTOs
{
    public class InventoryDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public bool IsPublic { get; set; }
        public string? ImageUrl { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
        public DateTime CreatedAtUtc { get; set; }
        public Guid CreatedByUserId { get; set; }
        public bool CanManageInventory { get; set; }
        public bool CanWriteItems { get; set; }
    }
}
