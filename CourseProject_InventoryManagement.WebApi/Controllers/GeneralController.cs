using Ardalis.Result.AspNetCore;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.GeneralCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.GeneralQueries;
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
        private readonly IGetDashboardStatistics _getDashboardStatistics;
        private readonly IGetLocalizationResources _getLocalizationResources;
        private readonly IGetLocalizationResourcesAdminList _getLocalizationResourcesAdminList;
        private readonly IUpsertLocalizationResource _upsertLocalizationResource;
        private readonly IBulkUpsertLocalizationResources _bulkUpsertLocalizationResources;
        private readonly IDeleteLocalizationResource _deleteLocalizationResource;

        public GeneralController(
            IGetDashboardStatistics getDashboardStatistics,
            IGetLocalizationResources getLocalizationResources,
            IGetLocalizationResourcesAdminList getLocalizationResourcesAdminList,
            IUpsertLocalizationResource upsertLocalizationResource,
            IBulkUpsertLocalizationResources bulkUpsertLocalizationResources,
            IDeleteLocalizationResource deleteLocalizationResource)
        {
            _getDashboardStatistics = getDashboardStatistics;
            _getLocalizationResources = getLocalizationResources;
            _getLocalizationResourcesAdminList = getLocalizationResourcesAdminList;
            _upsertLocalizationResource = upsertLocalizationResource;
            _bulkUpsertLocalizationResources = bulkUpsertLocalizationResources;
            _deleteLocalizationResource = deleteLocalizationResource;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<GetDashboardStatisticsResult>> GetDashboardStatistics(GetDashboardStatisticsQuery query, CancellationToken cancellationToken)
        {
            var result = await _getDashboardStatistics.GetDashboardStatistics(query, cancellationToken);
            return this.ToActionResult(result);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<GetLocalizationResourcesResult>> GetLocalizationResources([FromQuery] GetLocalizationResourcesQuery query, CancellationToken cancellationToken)
        {
            var result = await _getLocalizationResources.GetLocalizationResources(query, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<LocalizationResourceAdminResult>>> GetLocalizationResourcesAdminList([FromQuery] GetLocalizationResourcesAdminListQuery query, CancellationToken cancellationToken)
        {
            var result = await _getLocalizationResourcesAdminList.GetLocalizationResourcesAdminList(query, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Guid>> UpsertLocalizationResource([FromBody] UpsertLocalizationResourceCommand command, CancellationToken cancellationToken)
        {
            var result = await _upsertLocalizationResource.UpsertLocalizationResource(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<List<Guid>>> BulkUpsertLocalizationResources([FromBody] BulkUpsertLocalizationResourcesCommand command, CancellationToken cancellationToken)
        {
            var result = await _bulkUpsertLocalizationResources.BulkUpsertLocalizationResources(command, cancellationToken);
            return this.ToActionResult(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Guid>> DeleteLocalizationResource([FromBody] DeleteLocalizationResourceCommand command, CancellationToken cancellationToken)
        {
            var result = await _deleteLocalizationResource.DeleteLocalizationResource(command, cancellationToken);
            return this.ToActionResult(result);
        }
    }
}
