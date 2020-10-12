using BlazorApp.Common.Models;
using BlazorApp.Common.Wrappers;
using BlazorApp.CommonUI.Services.Implementations;
using MatBlazor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorApp.CommonUI.PageModels.Account
{
    [Authorize]
    public class ProfilePageModel : ComponentBase
    {
        [Parameter] public string UserId { get; set; }

        [Inject] private HttpClient Http { get; set; }

        [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; }

        [Inject] private IMatToaster MatToaster { get; set; }

        private ApiResponse _apiResponse;

        protected UserInfoModel UserInfo { get; set; }

        protected bool ResetPasswordDialogOpen { get; set; }

        protected RegisterModel RegisterParameters { get; } = new RegisterModel();

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
                _apiResponse =
                    await Http.PostJsonAsync<ApiResponse>($"api/Account/UserPasswordReset/{UserInfo.UserId}",
                        RegisterParameters.Password);
                if (_apiResponse.StatusCode == StatusCodes.Status204NoContent || _apiResponse.StatusCode == StatusCodes.Status200OK)
                {
                    MatToaster.Add("Password Reset", MatToastType.Success, _apiResponse.Message);
                }
                else
                {
                    MatToaster.Add(_apiResponse.Message, MatToastType.Danger);
                }
                ResetPasswordDialogOpen = false;
            }
        }

        protected async Task UpdateUserAsync()
        {
            try
            {
                var apiResponse = await ((IdentityAuthenticationStateProvider)AuthStateProvider).UpdateUser(UserInfo);
                if (apiResponse.StatusCode == StatusCodes.Status200OK)
                {
                    MatToaster.Add("Profile update was Successful", MatToastType.Success);
                }
                else
                {
                    MatToaster.Add(apiResponse.Message, MatToastType.Danger, "Profile Update Failed");
                }
            }
            catch (Exception ex)
            {
                MatToaster.Add(ex.Message, MatToastType.Danger, "Profile Update Failed");
            }
        }
    }
}