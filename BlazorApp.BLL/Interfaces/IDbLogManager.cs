using BlazorApp.Common.Wrappers;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorApp.BLL.Interfaces
{
    public interface IDbLogManager
    {
        Task<ApiResponse> GetAsync(int pageSize, int page, CancellationToken cancellationToken = default);
    }
}