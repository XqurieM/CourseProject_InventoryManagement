using Ardalis.Result.AspNetCore;
using CourseProject_InventoryManagement.Application.Features.CQRS;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.CommentCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.InventoryCommands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static CourseProject_InventoryManagement.Application.Features.CQRS.ICQRS;

namespace CourseProject_InventoryManagement.WebApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICQRS.ICreateNewComment _createNewComment;

        public CommentController(ICQRS.ICreateNewComment createNewComment)
        {
            _createNewComment = createNewComment;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Guid>> CreateNewComment(CreateNewCommentCommand command, CancellationToken cancellationToken)
        {
            var result = await _createNewComment.CreateNewComment(command, cancellationToken);
            return this.ToActionResult(result);
        }
    }
}
