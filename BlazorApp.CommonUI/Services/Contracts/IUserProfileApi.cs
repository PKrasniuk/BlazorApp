using BlazorApp.Common.Models;
using BlazorApp.Common.Wrappers;
using System.Threading.Tasks;

namespace BlazorApp.CommonUI.Services.Contracts
{
    public interface IUserProfileApi
    {
        Task<ApiResponse> UpsertAsync(UserProfileModel model);

        Task<ApiResponse> GetUserProfilesAsync();
    }
}