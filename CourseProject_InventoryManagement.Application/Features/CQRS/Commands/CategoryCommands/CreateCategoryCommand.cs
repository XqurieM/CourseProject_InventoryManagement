using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.CategoryCommands
{
    public class CreateCategoryCommand
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
    }
}
