using System;
using System.Threading.Tasks;
using BlazorApp.Common.Models;
using BlazorApp.CommonUI.Services.Contracts;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace BlazorApp.CommonUI.Services;

public class AppState
{
    private readonly IUserProfileApi _userProfileApi;

    public AppState(IUserProfileApi userProfileApi)
    {
        _userProfileApi = userProfileApi;
    }

    public UserProfileModel UserProfile { get; set; }

    public bool IsNavOpen
    {
        get => UserProfile == null || UserProfile.IsNavOpen;
        set => UserProfile.IsNavOpen = value;
    }

    public bool IsNavMinified { get; set; }
    public event Action OnChange;

    public async Task UpdateUserProfile()
    {
        await _userProfileApi.UpsertAsync(UserProfile);
    }

    public async Task<UserProfileModel> GetUserProfile()
    {
        if (UserProfile != null && !string.IsNullOrEmpty(UserProfile.UserId)) return UserProfile;

        var apiResponse = await _userProfileApi.GetUserProfilesAsync();
        return apiResponse.StatusCode == StatusCodes.Status200OK
            ? JsonConvert.DeserializeObject<UserProfileModel>(apiResponse.Result.ToString())
            : new UserProfileModel();
    }

    public async Task UpdateUserProfileCount(int count)
    {
        UserProfile.Count = count;
        await UpdateUserProfile();
        NotifyStateChanged();
    }

    public async Task<int> GetUserProfileCount()
    {
        if (UserProfile == null)
        {
            UserProfile = await GetUserProfile();
            return UserProfile.Count;
        }

        return UserProfile.Count;
    }

    public async Task SaveLastVisitedUri(string uri)
    {
        if (UserProfile == null)
        {
            UserProfile = await GetUserProfile();
        }
        else
        {
            UserProfile.LastPageVisited = uri;
            await UpdateUserProfile();
            NotifyStateChanged();
        }
    }

    private void NotifyStateChanged()
    {
        OnChange?.Invoke();
    }
}