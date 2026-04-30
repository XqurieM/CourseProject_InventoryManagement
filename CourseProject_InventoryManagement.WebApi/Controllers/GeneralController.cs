using Ardalis.Result.AspNetCore;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.GeneralQueries;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.InventoryQueries;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results.GeneralResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static CourseProject_InventoryManagement.Application.Features.CQRS.ICQRS;

namespace CourseProject_InventoryManagement.WebApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class GeneralController : ControllerBase
    {
        private readonly ICQRS.IGetDashboardStatistics _getDashboardStatistics;

        public GeneralController(IGetDashboardStatistics getDashboardStatistics)
        {
            _getDashboardStatistics = getDashboardStatistics;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<GetDashboardStatisticsResult>> GetDashboardStatistics(GetDashboardStatisticsQuery query, CancellationToken cancellationToken)
        {
            var result = await _getDashboardStatistics.GetDashboardStatistics(query, cancellationToken);
            return this.ToActionResult(result);
        }
    }
}