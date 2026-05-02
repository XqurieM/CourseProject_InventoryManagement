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
    public class DeleteLocalizationResourceCommandHandler : ICQRS.IDeleteLocalizationResource
    {
        private readonly IAppDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly ILocalizationCacheService _localizationCacheService;

        public DeleteLocalizationResourceCommandHandler(
            IAppDbContext context,
            IAuthenticatedUserService authenticatedUserService,
            ILocalizationCacheService localizationCacheService)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _localizationCacheService = localizationCacheService;
        }

        public async Task<Result<Guid>> DeleteLocalizationResource(DeleteLocalizationResourceCommand command, CancellationToken cancellationToken = default)
        {
            var adminResult = await _authenticatedUserService.GetRequiredAdminAsync(cancellationToken);
            if (!adminResult.IsSuccess)
            {
                return ResultFailureMapper.MapFailure<AppUser, Guid>(adminResult);
            }

            var entity = await _context.LocalizationResources.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
            if (entity is null)
            {
                return Result<Guid>.NotFound("Localization resource not found.");
            }

            var languageCode = entity.LanguageCode;
            _context.LocalizationResources.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
            _localizationCacheService.Invalidate(languageCode);

            return Result<Guid>.Success(command.Id);
        }
    }
}
