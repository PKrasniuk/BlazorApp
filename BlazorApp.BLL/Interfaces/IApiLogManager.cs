using System.Threading.Tasks;
using BlazorApp.Common.Models;
using BlazorApp.Common.Wrappers;

namespace BlazorApp.BLL.Interfaces;

public interface IApiLogManager
{
    Task<ApiResponse> LogAsync(ApiLogItemModel apiLogItem);

    Task<ApiResponse> GetApiResponsesAsync();

    Task<ApiResponse> GetApiResponseByApplicationUserIdAsync(string applicationUserId);
}