using Ardalis.Result.AspNetCore;
using CourseProject_InventoryManagement.Application.Abstractions.Storage;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.FilesCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.GeneralCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.GeneralQueries;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results.GeneralResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static CourseProject_InventoryManagement.Application.Features.CQRS.ICQRS;

namespace CourseProject_InventoryManagement.WebApi.Controllers
{
    public sealed class UploadInventoryImageRequest
    {
        public IFormFile? File { get; set; }
    }

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
        private readonly IUploadFile _uploadFile;
        private readonly ITelegramStorageProxy _telegramProxy;

        public GeneralController(IGetDashboardStatistics getDashboardStatistics, IGetLocalizationResources getLocalizationResources, IGetLocalizationResourcesAdminList getLocalizationResourcesAdminList, IUpsertLocalizationResource upsertLocalizationResource, IBulkUpsertLocalizationResources bulkUpsertLocalizationResources, IDeleteLocalizationResource deleteLocalizationResource, IUploadFile uploadFile, ITelegramStorageProxy telegramProxy)
        {
            _getDashboardStatistics = getDashboardStatistics;
            _getLocalizationResources = getLocalizationResources;
            _getLocalizationResourcesAdminList = getLocalizationResourcesAdminList;
            _upsertLocalizationResource = upsertLocalizationResource;
            _bulkUpsertLocalizationResources = bulkUpsertLocalizationResources;
            _deleteLocalizationResource = deleteLocalizationResource;
            _uploadFile = uploadFile;
            _telegramProxy = telegramProxy;
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

        [Authorize]
        [HttpPost]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<ActionResult<UploadedFileResultDto>> UploadInventoryImage([FromForm] UploadInventoryImageRequest request, CancellationToken cancellationToken)
        {
            var result = await _uploadFile.UploadFile(new UploadFileCommand(request.File!), cancellationToken);
            return this.ToActionResult(result);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ProxyTelegramImage([FromQuery] string fileId, CancellationToken cancellationToken)
        {
            var result = await _telegramProxy.GetFileDirectUrlAsync(fileId, cancellationToken);
            if (result.IsSuccess)
            {
                return Redirect(result.Value);
            }
            return NotFound("Image could not be retrieved from Telegram.");
        }
    }
}
