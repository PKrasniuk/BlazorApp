using System;
using System.Net;
using System.Threading.Tasks;
using BlazorApp.Common.Models;
using BlazorApp.CommonUI.Services.Implementations;
using MatBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorApp.CommonUI.PageModels.Account;

public class ResetPasswordPageModel : ComponentBase
{
    [Parameter] public string UserId { get; set; }

    [Inject] private NavigationManager NavigationManager { get; set; }

    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; }

    [Inject] private IMatToaster MatToaster { get; set; }

    protected ResetPasswordModel ResetPasswordParameters { get; } = new();

    protected override void OnInitialized()
    {
        var absoluteUrl = NavigationManager.Uri;
        var token = absoluteUrl.Substring(absoluteUrl.IndexOf("?token=", StringComparison.Ordinal) + 7);

        if (!string.IsNullOrEmpty(UserId) && !string.IsNullOrEmpty(token))
        {
            ResetPasswordParameters.Token = token;
            ResetPasswordParameters.UserId = UserId;
        }
        else
        {
            MatToaster.Add("Your url is missing the Reset Token. Fatal Error", MatToastType.Danger,
                "Reset Token is Missing");
        }
    }

    protected async Task SendResetPasswordAsync()
    {
        try
        {
            var apiResponse =
                await ((IdentityAuthenticationStateProvider)AuthStateProvider).ResetPassword(
                    ResetPasswordParameters);
            if (apiResponse.StatusCode == (int)HttpStatusCode.OK)
            {
                MatToaster.Add("Reset Password was Successful", MatToastType.Success);
                NavigationManager.NavigateTo("/account/login");
            }
            else
            {
                MatToaster.Add(apiResponse.Message, MatToastType.Danger, "Reset Password Failed");
            }
        }
        catch (Exception ex)
        {
            MatToaster.Add(ex.Message, MatToastType.Danger, "Reset Password Failed");
        }
    }
}