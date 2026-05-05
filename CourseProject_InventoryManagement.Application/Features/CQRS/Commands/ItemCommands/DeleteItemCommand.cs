using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.ItemCommands
{
    public class DeleteItemCommand
    {
        public Guid Id { get; set; }
    }
}
