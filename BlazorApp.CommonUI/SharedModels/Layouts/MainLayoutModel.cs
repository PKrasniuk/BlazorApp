using BlazorApp.CommonUI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Threading.Tasks;

namespace BlazorApp.CommonUI.SharedModels.Layouts
{
    public class MainLayoutModel : LayoutComponentBase
    {
        [CascadingParameter] private Task<AuthenticationState> AuthenticationStateTask { get; set; }

        [Inject] private NavigationManager NavigationManager { get; set; }

        [Inject] private AppState AppState { get; set; }

        protected bool NavMenuOpened { get; set; } = true;

        protected string BbDrawerClass { get; set; } = "";

        private bool NavMinified { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var user = (await AuthenticationStateTask).User;

            if (user.Identity.IsAuthenticated)
            {
                var profile = await AppState.GetUserProfile();
                NavMenuOpened = profile.IsNavOpen;
                NavMinified = profile.IsNavMinified;
            }
        }

        protected void CallLogin()
        {
            var returnUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
            NavigationManager.NavigateTo($"/account/Login/{returnUrl}");
        }

        protected void NavToggle()
        {
            NavMenuOpened = !NavMenuOpened;
            BbDrawerClass = NavMenuOpened ? "full" : "closed";

            StateHasChanged();
        }

        protected void NavMinify()
        {
            NavMinified = !NavMinified || !NavMenuOpened;

            if (NavMinified)
            {
                BbDrawerClass = "mini";
                NavMenuOpened = true;
            }
            else if (NavMenuOpened)
            {
                BbDrawerClass = "full";
            }

            NavMenuOpened = true;

            StateHasChanged();
        }
    }
}