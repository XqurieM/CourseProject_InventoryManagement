using Ardalis.Result.AspNetCore;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.GeneralQueries;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results.GeneralResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using static CourseProject_InventoryManagement.Application.Features.CQRS.ICQRS;

namespace CourseProject_InventoryManagement.WebApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly IGlobalSearch _globalSearch;

        public SearchController(IGlobalSearch globalSearch)
        {
            _globalSearch = globalSearch;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<GlobalSearchResult>> GlobalSearch([FromQuery] string q, CancellationToken cancellationToken)
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

            var query = new GlobalSearchQuery
            {
                SearchTerm = q,
                UserId = userId
            };

            var result = await _globalSearch.GlobalSearch(query, cancellationToken);
            return this.ToActionResult(result);
        }
    }
}
