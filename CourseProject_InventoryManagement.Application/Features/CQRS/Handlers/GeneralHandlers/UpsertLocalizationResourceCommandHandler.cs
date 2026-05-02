using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Localization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.GeneralCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results;
using CourseProject_InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.GeneralHandlers
{
    public class UpsertLocalizationResourceCommandHandler : ICQRS.IUpsertLocalizationResource
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly ILocalizationCacheService _localizationCacheService;

        public UpsertLocalizationResourceCommandHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            ILocalizationCacheService localizationCacheService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _localizationCacheService = localizationCacheService;
        }

        public async Task<Result<Guid>> UpsertLocalizationResource(UpsertLocalizationResourceCommand command, CancellationToken cancellationToken = default)
        {
            var adminResult = await _authenticatedUserService.GetRequiredAdminAsync(cancellationToken);
            if (!adminResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, Guid>(adminResult);
            }

            if (string.IsNullOrWhiteSpace(command.ResourceKey) ||
                string.IsNullOrWhiteSpace(command.LanguageCode) ||
                string.IsNullOrWhiteSpace(command.PageName))
            {
                return Result<Guid>.Invalid(new List<ValidationError>
                {
                    new() { Identifier = nameof(command.ResourceKey), ErrorMessage = "ResourceKey, LanguageCode and PageName are required." }
                });
            }

            var resourceKey = command.ResourceKey.Trim();
            var languageCode = LocalizationLanguageCodeHelper.NormalizeOrDefault(command.LanguageCode);
            var pageName = command.PageName.Trim();
            var value = command.Value?.Trim() ?? string.Empty;

            LocalizationResources? entity = null;

            if (command.Id.HasValue && command.Id.Value != Guid.Empty)
            {
                entity = await _context.LocalizationResources.FirstOrDefaultAsync(x => x.Id == command.Id.Value, cancellationToken);
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
                    IsActive = command.IsActive,
                    CreatedByUserId = adminResult.Value.Id
                };

                await _context.LocalizationResources.AddAsync(entity, cancellationToken);
            }
            else
            {
                entity.ResourceKey = resourceKey;
                entity.LanguageCode = languageCode;
                entity.PageName = pageName;
                entity.Value = value;
                entity.IsActive = command.IsActive;
                entity.UpdatedByUserId = adminResult.Value.Id;
            }

            await _context.SaveChangesAsync(cancellationToken);
            _localizationCacheService.Invalidate(languageCode);

            return Result<Guid>.Success(entity.Id);
        }
    }
}
