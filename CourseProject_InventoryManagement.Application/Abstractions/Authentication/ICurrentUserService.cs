namespace CourseProject_InventoryManagement.Application.Abstractions.Authentication
{
    public interface ICurrentUserService
    {
        bool IsAuthenticated { get; }
        Guid? UserId { get; }
    }
}
