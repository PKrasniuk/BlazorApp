using BlazorApp.Common.Models;
using BlazorApp.CommonUI.Services.Implementations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Threading.Tasks;

namespace BlazorApp.CommonUI.SharedModels.Components
{
    public class UserProfileModel : ComponentBase
    {
        [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; }

        [CascadingParameter] private Task<AuthenticationState> AuthenticationStateTask { get; set; }

        protected UserInfoModel UserInfoModel { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            UserInfoModel = null;

            var authenticationState = await AuthenticationStateTask;
            if (authenticationState != null)
            {
                var user = authenticationState.User;
                if (user.Identity.IsAuthenticated)
                {
                    UserInfoModel = await ((IdentityAuthenticationStateProvider)AuthStateProvider).GetUserInfo();
                }
            }
        }
    }
}