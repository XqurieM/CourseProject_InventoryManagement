using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;

namespace CourseProject_InventoryManagement.Application.Abstractions.Integrations
{
    public interface IDropboxService
    {
        Task<Result<string>> UploadFileAsync(string fileName, string fileContent, CancellationToken cancellationToken = default);
    }
}
