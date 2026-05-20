using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.DTOs
{
    public class ItemDto
    {
        public Guid Id { get; set; }
        public Guid InventoryId { get; set; }
        public string CustomId { get; set; }
        public byte[] RowVersion { get; set; }
        public DateTime CreateAtUtc { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public Guid UpdatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime DeletedAtUtc { get; set; }
        public string ItemName { get; set; }
        public int LikeCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public List<ItemImageDto> Images { get; set; } = new();
    }
}
