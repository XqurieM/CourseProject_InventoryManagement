namespace CourseProject_InventoryManagement.Application.Features.CQRS.Queries.TagQueries
{
    public class GetInventoriesByTagQuery
    {
        public Guid TagId { get; set; }
        public Guid? UserId { get; set; }
    }
}
