using Ardalis.Result.AspNetCore;
using CourseProject_InventoryManagement.Application.DTOs;
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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public GeneralController(
            IGetDashboardStatistics getDashboardStatistics,
            IGetLocalizationResources getLocalizationResources,
            IGetLocalizationResourcesAdminList getLocalizationResourcesAdminList,
            IUpsertLocalizationResource upsertLocalizationResource,
            IBulkUpsertLocalizationResources bulkUpsertLocalizationResources,
            IDeleteLocalizationResource deleteLocalizationResource,
            IWebHostEnvironment webHostEnvironment)
        {
            _getDashboardStatistics = getDashboardStatistics;
            _getLocalizationResources = getLocalizationResources;
            _getLocalizationResourcesAdminList = getLocalizationResourcesAdminList;
            _upsertLocalizationResource = upsertLocalizationResource;
            _bulkUpsertLocalizationResources = bulkUpsertLocalizationResources;
            _deleteLocalizationResource = deleteLocalizationResource;
            _webHostEnvironment = webHostEnvironment;
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
        public async Task<ActionResult<UploadedFileResultDto>> UploadInventoryImage(IFormFile file, CancellationToken cancellationToken)
        {
            if (file is null || file.Length == 0)
            {
                return BadRequest("An image file is required.");
            }

            if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Only image uploads are allowed.");
            }

            var webRoot = _webHostEnvironment.WebRootPath ?? Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");
            var uploadsRoot = Path.Combine(webRoot, "uploads", "inventories");
            Directory.CreateDirectory(uploadsRoot);

            var extension = Path.GetExtension(file.FileName);
            var safeFileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(uploadsRoot, safeFileName);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            var relativePath = $"/uploads/inventories/{safeFileName}";
            var absoluteUrl = $"{Request.Scheme}://{Request.Host}{relativePath}";

            return Ok(new UploadedFileResultDto
            {
                FileName = safeFileName,
                RelativePath = relativePath,
                Url = absoluteUrl
            });
        }
    }
}
