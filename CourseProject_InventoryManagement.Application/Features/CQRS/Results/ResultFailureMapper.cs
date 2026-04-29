using Ardalis.Result;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Results
{
    public static class ResultFailureMapper
    {
        public static Result<TDestination> MapFailure<TSource, TDestination>(Result<TSource> source)
        {
            return source.Status switch
            {
                ResultStatus.Unauthorized => Result<TDestination>.Unauthorized(source.Errors.ToArray()),
                ResultStatus.Forbidden => Result<TDestination>.Forbidden(source.Errors.ToArray()),
                ResultStatus.NotFound => Result<TDestination>.NotFound(source.Errors.ToArray()),
                ResultStatus.Conflict => Result<TDestination>.Conflict(source.Errors.ToArray()),
                ResultStatus.Invalid => Result<TDestination>.Invalid(source.ValidationErrors),
                _ => Result<TDestination>.Error(string.Join("; ", source.Errors))
            };
        }
    }
}
