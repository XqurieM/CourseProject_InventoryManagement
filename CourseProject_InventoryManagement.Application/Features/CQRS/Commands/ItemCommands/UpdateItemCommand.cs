using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.ItemCommands
{
    public class UpdateItemCommand
    {
        public Guid Id { get; set; }
        public Guid InventoryId { get; set; }
        public string CustomId { get; set; }
        public string ItemName { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
