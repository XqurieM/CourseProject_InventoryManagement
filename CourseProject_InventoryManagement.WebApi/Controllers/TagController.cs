using Ardalis.Result.AspNetCore;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.TagCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.TagQueries;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results.InventoryResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static CourseProject_InventoryManagement.Application.Features.CQRS.ICQRS;

namespace CourseProject_InventoryManagement.WebApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly IGetAllTags _getAllTags;
        private readonly IGetTagById _getTagById;
        private readonly ISearchTags _searchTags;
        private readonly IGetInventoriesByTag _getInventoriesByTag;
        private readonly ICreateTag _createTag;
        private readonly IUpdateTag _updateTag;
        private readonly IDeleteTag _deleteTag;

        public TagController(
            IGetAllTags getAllTags,
            IGetTagById getTagById,
            ISearchTags searchTags,
            IGetInventoriesByTag getInventoriesByTag,
            ICreateTag createTag,
            IUpdateTag updateTag,
            IDeleteTag deleteTag)
        {
            _getAllTags = getAllTags;
            _getTagById = getTagById;
            _searchTags = searchTags;
            _getInventoriesByTag = getInventoriesByTag;
            _createTag = createTag;
            _updateTag = updateTag;
            _deleteTag = deleteTag;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<TagDto>>> GetAllTags(CancellationToken cancellationToken)
        {
            var result = await _getAllTags.GetAllTags(cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<TagDto>> GetTagById(Guid tagId, CancellationToken cancellationToken)
        {
            var result = await _getTagById.GetTagById(tagId, cancellationToken);
            return this.ToActionResult(result);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<TagDto>>> SearchTags([FromQuery] string q, CancellationToken cancellationToken)
        {
            var result = await _searchTags.SearchTags(new SearchTagsQuery { SearchTerm = q }, cancellationToken);
            return this.ToActionResult(result);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<GetInventoriesWithJoinInfosResult>>> GetInventoriesByTag(Guid tagId, CancellationToken cancellationToken)
        {
            Guid? userId = null;

            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (Guid.TryParse(userIdClaim, out var parsedId))
                {
                    userId = parsedId;
                }
            }

            var result = await _getInventoriesByTag.GetInventoriesByTag(new GetInventoriesByTagQuery
            {
                TagId = tagId,
                UserId = userId
            }, cancellationToken);

            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Guid>> CreateTag(CreateTagCommand command, CancellationToken cancellationToken)
        {
            var result = await _createTag.CreateTag(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Guid>> UpdateTag(UpdateTagCommand command, CancellationToken cancellationToken)
        {
            var result = await _updateTag.UpdateTag(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Guid>> DeleteTag(DeleteTagCommand command, CancellationToken cancellationToken)
        {
            var result = await _deleteTag.DeleteTag(command, cancellationToken);
            return this.ToActionResult(result);
        }
    }
}
