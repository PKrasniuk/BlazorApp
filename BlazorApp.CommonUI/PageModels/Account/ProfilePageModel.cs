using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BlazorApp.Common.Models;
using BlazorApp.CommonUI.Services.Implementations;
using MatBlazor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;

namespace BlazorApp.CommonUI.PageModels.Account;

[Authorize]
public class ProfilePageModel : ComponentBase
{
    [Parameter] public string UserId { get; set; }

    [Inject] private HttpClient Http { get; set; }

    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; }

    [Inject] private IMatToaster MatToaster { get; set; }

    protected UserInfoModel UserInfo { get; set; }

    protected bool ResetPasswordDialogOpen { get; set; }

    protected RegisterModel RegisterParameters { get; } = new();

    protected override async Task OnInitializedAsync()
    {
        UserInfo = await ((IdentityAuthenticationStateProvider)AuthStateProvider).GetUserInfo();
    }

    protected void OpenResetPasswordDialog()
    {
        MatToaster.Add("Not Yet Implemented", MatToastType.Warning);
    }

    protected async Task ResetUserPasswordAsync()
    {
        if (RegisterParameters.Password != RegisterParameters.PasswordConfirm)
        {
            MatToaster.Add("Passwords Must Match", MatToastType.Warning);
        }
        else
        {
            var apiResponse =
                await Http.PostAsJsonAsync($"api/Account/UserPasswordReset/{UserInfo.UserId}",
                    RegisterParameters.Password);
            if (apiResponse.StatusCode == HttpStatusCode.NoContent || apiResponse.StatusCode == HttpStatusCode.OK)
                MatToaster.Add("Password Reset", MatToastType.Success,
                    await apiResponse.Content.ReadAsStringAsync());
            else
                MatToaster.Add(apiResponse.ReasonPhrase, MatToastType.Danger);
            ResetPasswordDialogOpen = false;
        }
    }

    protected async Task UpdateUserAsync()
    {
        try
        {
            var apiResponse = await ((IdentityAuthenticationStateProvider)AuthStateProvider).UpdateUser(UserInfo);
            if (apiResponse.StatusCode == StatusCodes.Status200OK)
                MatToaster.Add("Profile update was Successful", MatToastType.Success);
            else
                MatToaster.Add(apiResponse.Message, MatToastType.Danger, "Profile Update Failed");
        }
        catch (Exception ex)
        {
            MatToaster.Add(ex.Message, MatToastType.Danger, "Profile Update Failed");
        }
    }
}