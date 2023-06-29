using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorApp.CommonUI.SharedModels.Components;

public class NavMenuModel : ComponentBase
{
    [Inject] public NavigationManager NavigationManager { get; set; }

    [CascadingParameter] private Task<AuthenticationState> AuthenticationStateTask { get; set; }

    protected bool IsLoggedIn { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        IsLoggedIn = false;

        var authenticationState = await AuthenticationStateTask;
        if (authenticationState != null)
        {
            var user = authenticationState.User;
            if (user.Identity.IsAuthenticated) IsLoggedIn = true;
        }
    }
}