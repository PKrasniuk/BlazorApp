using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BlazorApp.Common.Models;
using BlazorApp.Common.Wrappers;
using BlazorApp.CommonUI.Services.Contracts;

namespace BlazorApp.CommonUI.Services.Implementations;

public class UserProfileApi : IUserProfileApi
{
    private readonly HttpClient _httpClient;

    public UserProfileApi(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse> UpsertAsync(UserProfileModel model)
    {
        var apiResponse = await _httpClient.PostAsJsonAsync("api/UserProfile/Upsert", model);
        return await ApiResponse.ReturnApiResponse(apiResponse);
    }

    public async Task<ApiResponse> GetUserProfilesAsync()
    {
        var apiResponse = await _httpClient.GetAsync("api/UserProfile");
        return await ApiResponse.ReturnApiResponse(apiResponse);
    }
}