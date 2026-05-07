using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.CategoryCommands
{
    public class DeleteCategoryCommand
    {
        public Guid CategoryId { get; set; }
    }
}
