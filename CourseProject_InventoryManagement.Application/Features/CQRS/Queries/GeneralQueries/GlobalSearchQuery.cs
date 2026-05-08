using System;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Queries.GeneralQueries
{
    public class GlobalSearchQuery
    {
        public string SearchTerm { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
    }
}
