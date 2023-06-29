using System.Threading.Tasks;
using BlazorApp.Common.Models;
using BlazorApp.Common.Wrappers;

namespace BlazorApp.CommonUI.Services.Contracts;

public interface IUserProfileApi
{
    Task<ApiResponse> UpsertAsync(UserProfileModel model);

    Task<ApiResponse> GetUserProfilesAsync();
}