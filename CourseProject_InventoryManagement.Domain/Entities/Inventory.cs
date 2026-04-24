using CourseProject_InventoryManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Domain.Entities
{
    public class Inventory : SoftDeletableEntity
    {
        public string Title { get; private set; } = null!;
        public string? Description { get; private set; }
        public Guid CategoryId { get; private set; }
        public string? ImageUrl { get; private set; }
        public bool IsPublic { get; private set; }
        public byte[] RowVersion { get; private set; } = Array.Empty<byte>();
        public Category Category { get; private set; } = null!;
        public ICollection<InventoryField> Fields { get; set; } = new List<InventoryField>();
        public ICollection<InventoryCustomIdRule> CustomIdRules { get; set; } = new List<InventoryCustomIdRule>();
        public ICollection<InventoryAccess> Accesses { get; set; } = new List<InventoryAccess>();
        public ICollection<Item> Items { get; set; } = new List<Item>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<InventoryTag> InventoryTags { get; set; } = new List<InventoryTag>();


    }
}
