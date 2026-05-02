using CourseProject_InventoryManagement.Application.DTOs;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Commands.GeneralCommands
{
    public class BulkUpsertLocalizationResourcesCommand
    {
        public List<LocalizationResourceUpsertDto> Resources { get; set; } = new();
    }
}
