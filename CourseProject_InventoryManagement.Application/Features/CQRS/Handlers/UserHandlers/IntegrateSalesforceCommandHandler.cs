using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Integrations;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.UserCommands;
using Microsoft.Extensions.Logging;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.UserHandlers
{
    public class IntegrateSalesforceCommandHandler : ICQRS.IIntegrateSalesforce
    {
        private readonly ISalesforceService _salesforceService;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly ILogger<IntegrateSalesforceCommandHandler> _logger;

        public IntegrateSalesforceCommandHandler(
            ISalesforceService salesforceService,
            IAuthenticatedUserService authenticatedUserService,
            ILogger<IntegrateSalesforceCommandHandler> logger)
        {
            _salesforceService = salesforceService;
            _authenticatedUserService = authenticatedUserService;
            _logger = logger;
        }

        public async Task<Result<SalesforceIntegrationResultDto>> IntegrateSalesforce(
            IntegrateSalesforceCommand command,
            CancellationToken cancellationToken = default)
        {
            var userResult = await _authenticatedUserService.GetRequiredUserAsync(cancellationToken);
            if (!userResult.IsSuccess)
            {
                return Result<SalesforceIntegrationResultDto>.Unauthorized();
            }

            var currentUser = userResult.Value;
            if (currentUser.Id != command.UserId && !currentUser.IsAdmin)
            {
                return Result<SalesforceIntegrationResultDto>.Forbidden("You are not allowed to integrate this user.");
            }

            _logger.LogInformation("Starting Salesforce integration for User {UserId} ({UserName}). Data: AccountName={AccountName}", 
                command.UserId, currentUser.UserName, command.AccountName);

            var result = await _salesforceService.IntegrateUserAsync(command, cancellationToken);
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully integrated User {UserId} into Salesforce. AccountId: {AccountId}, ContactId: {ContactId}",
                    command.UserId, result.Value.AccountId, result.Value.ContactId);
            }
            else
            {
                _logger.LogError("Salesforce integration failed for User {UserId}. Errors: {Errors}",
                    command.UserId, string.Join(", ", result.Errors));
            }

            return result;
        }
    }
}
