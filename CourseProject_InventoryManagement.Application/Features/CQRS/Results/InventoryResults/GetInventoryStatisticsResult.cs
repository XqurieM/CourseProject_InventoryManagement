namespace CourseProject_InventoryManagement.Application.Features.CQRS.Results.InventoryResults
{
    public class GetInventoryStatisticsResult
    {
        public Guid InventoryId { get; set; }
        public int ItemCount { get; set; }
        public int CommentCount { get; set; }
        public int FieldCount { get; set; }
        public int TagCount { get; set; }
        public int LikeCount { get; set; }
        public int ExplicitAccessCount { get; set; }
    }
}
