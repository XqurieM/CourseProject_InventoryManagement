using Ardalis.Result.AspNetCore;
using CourseProject_InventoryManagement.Application.DTOs;
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
        private readonly ICQRS.IGetInventoryComments _getInventoryComments;
        private readonly ICQRS.IDeleteComment _deleteComment;
        public CommentController(ICreateNewComment createNewComment, IGetInventoryComments getInventoryComments, IDeleteComment deleteComment)
        {
            _createNewComment = createNewComment;
            _getInventoryComments = getInventoryComments;
            _deleteComment = deleteComment;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Guid>> CreateNewComment(CreateNewCommentCommand command, CancellationToken cancellationToken)
        {
            var result = await _createNewComment.CreateNewComment(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<CommentDto>>> GetInventoryComments(Guid inventoryId, CancellationToken cancellationToken)
        {
            var result = await _getInventoryComments.GetInventoryComments(inventoryId, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Guid>> DeleteComment(Guid commentId, CancellationToken cancellationToken)
        {
            var result = await _deleteComment.DeleteComment(commentId, cancellationToken);
            return this.ToActionResult(result);
        }
    }
}
