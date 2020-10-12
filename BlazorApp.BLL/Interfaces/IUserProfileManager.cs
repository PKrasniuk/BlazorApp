using BlazorApp.Common.Models;
using BlazorApp.Common.Wrappers;
using System.Threading.Tasks;

namespace BlazorApp.BLL.Interfaces
{
    public interface IUserProfileManager
    {
        Task<ApiResponse> GetUserProfileAsync();

        Task<ApiResponse> GetUserProfileAsync(string userId);

        Task<ApiResponse> UpsertUserProfileAsync(UserProfileModel userProfile);

        Task<ApiResponse> GetLastPageVisitedAsync(string userName);
    }
}