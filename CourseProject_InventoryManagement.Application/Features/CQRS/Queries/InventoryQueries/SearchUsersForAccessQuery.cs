namespace CourseProject_InventoryManagement.Application.Features.CQRS.Queries.InventoryQueries
{
    public class SearchUsersForAccessQuery
    {
        public Guid InventoryId { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
    }
}
