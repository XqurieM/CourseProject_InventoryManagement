namespace CourseProject_InventoryManagement.Application.Features.CQRS.Queries.TagQueries
{
    public class GetPopularTagsQuery
    {
        public int Take { get; set; } = 20;
    }
}
