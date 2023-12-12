using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BlazorApp.Common.Models;
using BlazorApp.Common.Wrappers;
using BlazorApp.CommonUI.Services;
using BlazorApp.CommonUI.Services.Implementations;
using MatBlazor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;

namespace BlazorApp.CommonUI.PageModels.Admin;

[Authorize]
public class UsersPageModel : ComponentBase
{
    [Inject] private HttpClient Http { get; set; }

    [Inject] private IMatToaster MatToaster { get; set; }

    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; }

    [Inject] private AppState AppState { get; set; }

    private int PageSize { get; } = 10;

    private int CurrentPage { get; } = 0;

    protected List<RoleSelection> RoleSelections { get; set; } = new();

    protected UserInfoModel User { get; set; } = new();

    protected RegisterModel RegisterParameters { get; set; } = new();

    protected bool CreateUserDialogOpen { get; set; }

    protected bool EditDialogOpen { get; set; }

    protected bool DeleteUserDialogOpen { get; set; }

    protected bool ResetPasswordDialogOpen { get; set; }

    protected List<UserInfoModel> Users { get; set; }

    protected UserProfileModel UserProfile { get; set; } = new();

    private async Task RetrieveUserListAsync()
    {
        try
        {
            var apiResponse = await Http.GetFromJsonAsync<ApiResponse>(
                $"api/Admin/Users?pageSize={PageSize}&pageNumber={CurrentPage}");
            if (apiResponse.StatusCode == (int)HttpStatusCode.OK)
            {
                MatToaster.Add(apiResponse.Message, MatToastType.Success, "Users Retrieved");
                Users = JsonConvert.DeserializeObject<UserInfoModel[]>(apiResponse.Result.ToString()).ToList();
            }
            else
            {
                MatToaster.Add($"{apiResponse.Message} : {apiResponse.StatusCode}", MatToastType.Danger,
                    "User Retrieval Failed");
            }
        }
        catch (Exception ex)
        {
            MatToaster.Add(ex.GetBaseException().Message, MatToastType.Danger, "User Retrieval Error");
        }
    }

    private async Task PopulateRoleListAsync()
    {
        try
        {
            var response = await Http.GetFromJsonAsync<ApiResponse>("api/Account/ListRoles");

            RoleSelections = new List<RoleSelection>();

            foreach (var role in JsonConvert.DeserializeObject<string[]>(response.Result.ToString()).ToList())
                RoleSelections.Add(new RoleSelection
                {
                    Name = role,
                    IsSelected = false
                });
        }
        catch (Exception ex)
        {
            MatToaster.Add(ex.GetBaseException().Message, MatToastType.Danger, "Role Retrieval Error");
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await RetrieveUserListAsync();
        await PopulateRoleListAsync();
        UserProfile = await AppState.GetUserProfile();
    }

    protected void OpenEditDialog(string userId)
    {
        User = Users.SingleOrDefault(x => x.UserId == userId) ?? new UserInfoModel();
        foreach (var role in RoleSelections) role.IsSelected = User.Roles.Contains(role.Name);

        EditDialogOpen = true;
    }

    protected void OpenResetPasswordDialog(string userName, string userId)
    {
        RegisterParameters = new RegisterModel
        {
            UserName = userName
        };
        User.UserId = userId;
        ResetPasswordDialogOpen = true;
    }

    protected void OpenDeleteDialog(string userId)
    {
        User = Users.SingleOrDefault(x => x.UserId == userId) ?? new UserInfoModel();
        DeleteUserDialogOpen = true;
    }

    protected static void UpdateUserRole(RoleSelection roleSelectionItem)
    {
        roleSelectionItem.IsSelected = !roleSelectionItem.IsSelected;
    }

    protected void CancelChanges()
    {
        EditDialogOpen = false;
    }

    protected async Task UpdateUserAsync()
    {
        try
        {
            User.Roles = RoleSelections.Where(x => x.IsSelected).Select(x => x.Name).ToList();

            var apiResponse = await Http.PutAsJsonAsync("api/Account", User);
            if (apiResponse.StatusCode == HttpStatusCode.OK)
                MatToaster.Add("User Updated", MatToastType.Success);
            else
                MatToaster.Add("Error", MatToastType.Danger, apiResponse.StatusCode.ToString());

            EditDialogOpen = false;
        }
        catch (Exception ex)
        {
            MatToaster.Add(ex.GetBaseException().Message, MatToastType.Danger, "User Update Error");
        }
    }

    protected async Task CreateUserAsync()
    {
        try
        {
            if (RegisterParameters.Password != RegisterParameters.PasswordConfirm)
            {
                MatToaster.Add("Password Confirmation Failed", MatToastType.Danger, "");
                return;
            }

            var apiResponse =
                await ((IdentityAuthenticationStateProvider)AuthStateProvider).Create(RegisterParameters);
            if (apiResponse.StatusCode == (int)HttpStatusCode.OK)
            {
                MatToaster.Add(apiResponse.Message, MatToastType.Success);
                User = JsonConvert.DeserializeObject<UserInfoModel>(apiResponse.Result.ToString());
                Users.Add(User);
                RegisterParameters = new RegisterModel();
                CreateUserDialogOpen = false;
            }
            else
            {
                MatToaster.Add($"{apiResponse.Message} : {apiResponse.StatusCode}", MatToastType.Danger,
                    "User Creation Failed");
            }
        }
        catch (Exception ex)
        {
            MatToaster.Add(ex.GetBaseException().Message, MatToastType.Danger, "User Creation Error");
        }
    }

    protected async Task ResetUserPasswordAsync()
    {
        try
        {
            if (RegisterParameters.Password != RegisterParameters.PasswordConfirm)
            {
                MatToaster.Add("Passwords Must Match", MatToastType.Warning);
            }
            else
            {
                var apiResponse = await Http.PostAsJsonAsync(
                    $"api/Account/AdminUserPasswordReset/{User.UserId}", RegisterParameters.Password);

                if (apiResponse.StatusCode == HttpStatusCode.OK)
                    MatToaster.Add("Password Reset", MatToastType.Success,
                        await apiResponse.Content.ReadAsStringAsync());
                else
                    MatToaster.Add(apiResponse.ReasonPhrase, MatToastType.Danger);

                ResetPasswordDialogOpen = false;
            }
        }
        catch (Exception ex)
        {
            MatToaster.Add(ex.GetBaseException().Message, MatToastType.Danger, "Password Reset Error");
        }
    }

    protected async Task DeleteUserAsync()
    {
        try
        {
            var response = await Http.DeleteAsync($"api/Account/{User.UserId}");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                MatToaster.Add("User Deleted", MatToastType.Success);
                Users.Remove(User);
                DeleteUserDialogOpen = false;
                StateHasChanged();
            }
            else
            {
                MatToaster.Add("User Delete Failed", MatToastType.Danger);
            }
        }
        catch (Exception ex)
        {
            MatToaster.Add(ex.GetBaseException().Message, MatToastType.Danger, "User Delete Error");
        }
    }

    protected class RoleSelection
    {
        public bool IsSelected { get; set; }

        public string Name { get; set; }
    }
}