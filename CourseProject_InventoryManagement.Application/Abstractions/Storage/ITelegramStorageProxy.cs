using Ardalis.Result;
using System.Threading;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Application.Abstractions.Storage
{
    public interface ITelegramStorageProxy
    {
        Task<Result<string>> GetFileDirectUrlAsync(string fileId, CancellationToken cancellationToken = default);
    }
}
