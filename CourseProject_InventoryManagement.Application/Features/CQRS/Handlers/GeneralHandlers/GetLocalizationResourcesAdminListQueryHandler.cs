using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.GeneralQueries;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results.GeneralResults;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.GeneralHandlers
{
    public class GetLocalizationResourcesAdminListQueryHandler : ICQRS.IGetLocalizationResourcesAdminList
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public GetLocalizationResourcesAdminListQueryHandler(IAppDbContext context, IAuthenticatedUserService authenticatedUserService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
        }

        public async Task<Result<List<LocalizationResourceAdminResult>>> GetLocalizationResourcesAdminList(GetLocalizationResourcesAdminListQuery query, CancellationToken cancellationToken = default)
        {
            var adminResult = await _authenticatedUserService.GetRequiredAdminAsync(cancellationToken);
            if (!adminResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, List<LocalizationResourceAdminResult>>(adminResult);
            }

            var dbQuery = _context.LocalizationResources.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.LanguageCode))
            {
                var languageCode = LocalizationLanguageCodeHelper.NormalizeOrDefault(query.LanguageCode);
                dbQuery = dbQuery.Where(x => x.LanguageCode == languageCode);
            }

            if (!string.IsNullOrWhiteSpace(query.PageName))
            {
                var pageName = query.PageName.Trim();
                dbQuery = dbQuery.Where(x => x.PageName == pageName);
            }

            if (!string.IsNullOrWhiteSpace(query.ResourceKey))
            {
                var resourceKey = query.ResourceKey.Trim();
                dbQuery = dbQuery.Where(x => x.ResourceKey.Contains(resourceKey));
            }

            if (query.IsActive.HasValue)
            {
                dbQuery = dbQuery.Where(x => x.IsActive == query.IsActive.Value);
            }

            var result = await dbQuery
                .OrderBy(x => x.PageName)
                .ThenBy(x => x.LanguageCode)
                .ThenBy(x => x.ResourceKey)
                .Select(x => new LocalizationResourceAdminResult
                {
                    Id = x.Id,
                    ResourceKey = x.ResourceKey,
                    LanguageCode = x.LanguageCode,
                    Value = x.Value,
                    IsActive = x.IsActive,
                    PageName = x.PageName,
                    CreatedAtUtc = x.CreatedAtUtc,
                    CreatedByUserId = x.CreatedByUserId,
                    UpdatedAtUtc = x.UpdatedAtUtc ?? x.CreatedAtUtc,
                    UpdatedByUserId = x.UpdatedByUserId ?? x.CreatedByUserId
                })
                .ToListAsync(cancellationToken);

            return Result<List<LocalizationResourceAdminResult>>.Success(result);
        }
    }
}
