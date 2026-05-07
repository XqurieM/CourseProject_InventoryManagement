using Ardalis.Result.AspNetCore;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results.InventoryResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static CourseProject_InventoryManagement.Application.Features.CQRS.ICQRS;

namespace CourseProject_InventoryManagement.WebApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly IGetAllTags _getAllTags;

        public TagController(IGetAllTags getAllTags)
        {
            _getAllTags = getAllTags;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<TagDto>>> GetAllTags(CancellationToken cancellationToken)
        {
            var result = await _getAllTags.GetAllTags(cancellationToken);
            return this.ToActionResult(result);
        }
    }
}
