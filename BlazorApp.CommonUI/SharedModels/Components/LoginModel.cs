using BlazorApp.CommonUI.Services.Implementations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Threading.Tasks;

namespace BlazorApp.CommonUI.SharedModels.Components
{
    public class LoginModel : ComponentBase
    {
        [Inject] private NavigationManager NavigationManager { get; set; }

        [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; }

        protected async Task LogoutClickAsync()
        {
            await ((IdentityAuthenticationStateProvider)AuthStateProvider).Logout();
            NavigationManager.NavigateTo("/account/login");
        }
    }
}