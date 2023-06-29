using System;
using System.Threading.Tasks;
using BlazorApp.Common.Constants;
using BlazorApp.Common.Models;
using BlazorApp.CommonUI.Services.Implementations;
using MatBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.JSInterop;

namespace BlazorApp.CommonUI.PageModels.Account;

public class LoginPageModel : ComponentBase
{
    [Parameter] public string ReturnUrl { get; set; }

    [CascadingParameter] private Task<AuthenticationState> AuthenticationStateTask { get; set; }

    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; }

    [Inject] private IJSRuntime JsRuntime { get; set; }

    [Inject] private NavigationManager NavigationManager { get; set; }

    [Inject] private IMatToaster MatToaster { get; set; }

    private string NavigateTo { get; set; }

    protected bool ForgotPasswordToggle { get; set; }

    protected LoginModel LoginParameters { get; } = new();

    protected ForgotPasswordModel ForgotPasswordParameters { get; } = new();

    protected override async Task OnParametersSetAsync()
    {
        var user = (await AuthenticationStateTask).User;
        if (user.Identity.IsAuthenticated) NavigationManager.NavigateTo(NavigationManager.BaseUri + NavigateTo, true);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender) await JsRuntime.InvokeVoidAsync("SetFocus", "userName");

        await base.OnAfterRenderAsync(firstRender);
    }

    protected void GoBack()
    {
        if (string.IsNullOrEmpty(ReturnUrl)) ReturnUrl = "";

        NavigationManager.NavigateTo(ReturnUrl);
    }

    protected void Register()
    {
        NavigationManager.NavigateTo("/account/register");
    }

    protected async Task SubmitLoginAsync()
    {
        try
        {
            var response = await ((IdentityAuthenticationStateProvider)AuthStateProvider).Login(LoginParameters);
            if (response.StatusCode == StatusCodes.Status200OK)
                NavigateTo = response.Result != null ? (string)response.Result : CommonConstants.DefaultPageVisited;
            else
                MatToaster.Add(response.Message, MatToastType.Danger, "Login Attempt Failed");
        }
        catch (Exception ex)
        {
            MatToaster.Add(ex.Message, MatToastType.Danger, "Login Attempt Failed");
        }
    }

    protected async Task ForgotPasswordAsync()
    {
        try
        {
            await ((IdentityAuthenticationStateProvider)AuthStateProvider).ForgotPassword(ForgotPasswordParameters);
            MatToaster.Add("Forgot Password Email Sent", MatToastType.Success);
            ForgotPasswordParameters.Email = "";
            ForgotPasswordToggle = false;
        }
        catch (Exception ex)
        {
            MatToaster.Add(ex.Message, MatToastType.Danger, "Reset Password Attempt Failed");
        }
    }
}