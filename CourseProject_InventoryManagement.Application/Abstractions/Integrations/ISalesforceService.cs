using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.UserCommands;

namespace CourseProject_InventoryManagement.Application.Abstractions.Integrations
{
    public interface ISalesforceService
    {
        Task<Result<SalesforceIntegrationResultDto>> IntegrateUserAsync(IntegrateSalesforceCommand command, CancellationToken cancellationToken = default);
    }
}
