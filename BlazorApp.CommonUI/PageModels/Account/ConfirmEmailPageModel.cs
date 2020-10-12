using BlazorApp.Common.Models;
using BlazorApp.CommonUI.Services.Implementations;
using MatBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Threading.Tasks;

namespace BlazorApp.CommonUI.PageModels.Account
{
    public class ConfirmEmailPageModel : ComponentBase
    {
        [Parameter] public string UserId { get; set; }

        [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; }

        [Inject] private NavigationManager NavigationManager { get; set; }

        [Inject] private IMatToaster MatToaster { get; set; }

        protected ConfirmEmailModel ConfirmEmailParameters { get; set; } = new ConfirmEmailModel();

        protected bool DisableConfirmButton { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var absoluteUrl = NavigationManager.Uri;
            var token = absoluteUrl.Substring(absoluteUrl.IndexOf("?token=", StringComparison.Ordinal) + 7);

            if (!string.IsNullOrEmpty(UserId) && !string.IsNullOrEmpty(token))
            {
                DisableConfirmButton = true;
                ConfirmEmailParameters = new ConfirmEmailModel
                {
                    Token = token,
                    UserId = UserId
                };
                await SendConfirmationAsync();
            }
        }

        protected async Task SendConfirmationAsync()
        {
            try
            {
                await ((IdentityAuthenticationStateProvider)AuthStateProvider).ConfirmEmail(ConfirmEmailParameters);
                MatToaster.Add("Account has been Approved and Activated", MatToastType.Success);
                NavigationManager.NavigateTo("/");
            }
            catch (Exception ex)
            {
                MatToaster.Add(ex.Message, MatToastType.Danger, "Email Verification Failed");
            }
        }
    }
}