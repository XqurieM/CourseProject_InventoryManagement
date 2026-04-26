using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.ItemCommands
{
    public class AddItemCommand
    {
        public Guid InventoryId { get; set; }
        public string CustomId { get; set; }
        public Guid CreatedByUserId { get; set; }
    }
}
