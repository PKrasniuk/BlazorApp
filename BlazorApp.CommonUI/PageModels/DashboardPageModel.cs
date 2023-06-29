using System.Threading.Tasks;
using BlazorApp.CommonUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.CommonUI.PageModels;

[Authorize]
public class DashboardPageModel : ComponentBase
{
    protected int ProfileCurrentCount = -1;
    [Inject] private AppState AppState { get; set; }

    protected int CurrentCount { get; set; }

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