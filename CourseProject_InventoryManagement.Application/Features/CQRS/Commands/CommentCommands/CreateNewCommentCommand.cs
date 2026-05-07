using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.CommentCommands
{
    public class CreateNewCommentCommand
    {
        public Guid InventoryId { get; set; }
        public string Content { get; set; }
    }
}
