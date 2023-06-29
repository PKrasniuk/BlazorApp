using System.Threading;
using System.Threading.Tasks;
using BlazorApp.Common.Wrappers;

namespace BlazorApp.BLL.Interfaces;

public interface IDbLogManager
{
    Task<ApiResponse> GetAsync(int pageSize, int page, CancellationToken cancellationToken = default);
}