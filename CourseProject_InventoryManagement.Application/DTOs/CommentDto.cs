using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.DTOs
{
    public class CommentDto
    {
        public Guid Id { get; set; }
        public Guid InventoryId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public Guid UpdatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime DeletedAtUtc { get; set; }

    }
}
