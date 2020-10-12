using BlazorApp.Common.Models;
using BlazorApp.Common.Wrappers;
using BlazorApp.CommonUI.Services.Contracts;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorApp.CommonUI.Services.Implementations
{
    public class UserProfileApi : IUserProfileApi
    {
        private readonly HttpClient _httpClient;

        public UserProfileApi(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResponse> UpsertAsync(UserProfileModel model)
        {
            return await _httpClient.PostJsonAsync<ApiResponse>("api/UserProfile/Upsert", model);
        }

        public async Task<ApiResponse> GetUserProfilesAsync()
        {
            return await _httpClient.GetJsonAsync<ApiResponse>("api/UserProfile");
        }
    }
}