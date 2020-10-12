using BlazorApp.CommonUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace BlazorApp.CommonUI.PageModels
{
    [Authorize]
    public class DashboardPageModel : ComponentBase
    {
        [Inject] private AppState AppState { get; set; }

        protected int CurrentCount { get; set; }

        protected int ProfileCurrentCount = -1;

        protected override async Task OnInitializedAsync()
        {
            ProfileCurrentCount = await AppState.GetUserProfileCount();
        }

        protected async Task IncrementCountAsync()
        {
            CurrentCount++;
            ProfileCurrentCount++;
            await AppState.UpdateUserProfileCount(ProfileCurrentCount);
        }
    }
}