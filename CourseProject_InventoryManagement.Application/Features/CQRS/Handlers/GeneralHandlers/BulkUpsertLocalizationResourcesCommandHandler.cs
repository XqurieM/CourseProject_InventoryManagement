using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Localization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.GeneralCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.GeneralHandlers
{
    public class BulkUpsertLocalizationResourcesCommandHandler : ICQRS.IBulkUpsertLocalizationResources
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly ILocalizationCacheService _localizationCacheService;

        public BulkUpsertLocalizationResourcesCommandHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            ILocalizationCacheService localizationCacheService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _localizationCacheService = localizationCacheService;
        }

        public async Task<Result<List<Guid>>> BulkUpsertLocalizationResources(BulkUpsertLocalizationResourcesCommand command, CancellationToken cancellationToken = default)
        {
            var adminResult = await _authenticatedUserService.GetRequiredAdminAsync(cancellationToken);
            if (!adminResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, List<Guid>>(adminResult);
            }

            if (command.Resources.Count == 0)
            {
                return Result<List<Guid>>.Invalid(new List<ValidationError>
                {
                    new() { Identifier = nameof(command.Resources), ErrorMessage = "At least one localization resource is required." }
                });
            }

            var resultIds = new List<Guid>();
            var invalidationLanguages = new HashSet<string>();

            foreach (var input in command.Resources)
            {
                var result = await UpsertSingleAsync(input, adminResult.Value.Id, cancellationToken);
                if (!result.IsSuccess)
                {
                    return ResultFailureMapper.MapFailure<Guid, List<Guid>>(result);
                }

                resultIds.Add(result.Value);
                invalidationLanguages.Add(LocalizationLanguageCodeHelper.NormalizeOrDefault(input.LanguageCode));
            }

            await _context.SaveChangesAsync(cancellationToken);
            _localizationCacheService.InvalidateMany(invalidationLanguages);

            return Result<List<Guid>>.Success(resultIds);
        }

        private async Task<Result<Guid>> UpsertSingleAsync(LocalizationResourceUpsertDto input, Guid adminUserId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(input.ResourceKey) ||
                string.IsNullOrWhiteSpace(input.LanguageCode) ||
                string.IsNullOrWhiteSpace(input.PageName))
            {
                return Result<Guid>.Invalid(new List<ValidationError>
                {
                    new() { Identifier = nameof(input.ResourceKey), ErrorMessage = "ResourceKey, LanguageCode and PageName are required." }
                });
            }

            var resourceKey = input.ResourceKey.Trim();
            var languageCode = LocalizationLanguageCodeHelper.NormalizeOrDefault(input.LanguageCode);
            var pageName = input.PageName.Trim();
            var value = input.Value?.Trim() ?? string.Empty;

            LocalizationResources? entity = null;

            if (input.Id.HasValue && input.Id.Value != Guid.Empty)
            {
                entity = await _context.LocalizationResources.FirstOrDefaultAsync(x => x.Id == input.Id.Value, cancellationToken);
            }

            entity ??= await _context.LocalizationResources.FirstOrDefaultAsync(
                x => x.ResourceKey == resourceKey &&
                     x.LanguageCode == languageCode &&
                     x.PageName == pageName,
                cancellationToken);

            if (entity is null)
            {
                entity = new LocalizationResources
                {
                    ResourceKey = resourceKey,
                    LanguageCode = languageCode,
                    PageName = pageName,
                    Value = value,
                    IsActive = input.IsActive,
                    CreatedByUserId = adminUserId
                };

                await _context.LocalizationResources.AddAsync(entity, cancellationToken);
            }
            else
            {
                entity.ResourceKey = resourceKey;
                entity.LanguageCode = languageCode;
                entity.PageName = pageName;
                entity.Value = value;
                entity.IsActive = input.IsActive;
                entity.UpdatedByUserId = adminUserId;
            }

            return Result<Guid>.Success(entity.Id);
        }
    }
}
