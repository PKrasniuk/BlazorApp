using System;
using System.Net;
using System.Threading.Tasks;
using BlazorApp.Common.Models;
using BlazorApp.CommonUI.Services.Implementations;
using MatBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorApp.CommonUI.PageModels.Account;

public class RegisterPageModel : ComponentBase
{
    [Inject] private NavigationManager NavigationManager { get; set; }

    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; }

    [Inject] private IMatToaster MatToaster { get; set; }

    protected RegisterModel RegisterParameters { get; } = new();

    protected async Task RegisterUserAsync()
    {
        try
        {
            var response =
                await ((IdentityAuthenticationStateProvider)AuthStateProvider).Register(RegisterParameters);
            if (response.StatusCode == (int)HttpStatusCode.OK)
            {
                MatToaster.Add($"New User Email Verification was sent to: {RegisterParameters.Email}",
                    MatToastType.Success, "User Creation Successful");
                NavigationManager.NavigateTo("");
            }
            else
            {
                MatToaster.Add(response.Message, MatToastType.Danger, "User Creation Failed");
            }
        }
        catch (Exception ex)
        {
            MatToaster.Add(ex.Message, MatToastType.Danger, "User Creation Failed");
        }
    }
}