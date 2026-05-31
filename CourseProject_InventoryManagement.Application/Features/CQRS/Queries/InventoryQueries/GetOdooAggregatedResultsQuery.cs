using System;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Queries.InventoryQueries
{
    public class GetOdooAggregatedResultsQuery
    {
        public string Token { get; set; } = null!;
    }
}
