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
        private readonly ICQRS.IUpdateComment _updateComment;
        public CommentController(ICreateNewComment createNewComment, IGetInventoryComments getInventoryComments, IDeleteComment deleteComment, IUpdateComment updateComment)
        {
            _createNewComment = createNewComment;
            _getInventoryComments = getInventoryComments;
            _deleteComment = deleteComment;
            _updateComment = updateComment;
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
        public async Task<ActionResult<Guid>> UpdateComment(UpdateCommentCommand command, CancellationToken cancellationToken)
        {
            var result = await _updateComment.UpdateComment(command, cancellationToken);
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
