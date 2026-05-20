using CourseProject_InventoryManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Domain.Entities
{
    public class Item : SoftDeletableEntity
    {
        public Guid InventoryId { get; set; }
        public string ItemName { get; set; }
        public string CustomId { get; set; } = null!;
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
        public Inventory Inventory { get; set; } = null!;
        public ICollection<ItemFieldValue> FieldValues { get; set; } = new List<ItemFieldValue>();
        public ICollection<ItemLike> Likes { get; set; } = new List<ItemLike>();
        public ICollection<ItemImage> Images { get; set; } = new List<ItemImage>();
    }
}
