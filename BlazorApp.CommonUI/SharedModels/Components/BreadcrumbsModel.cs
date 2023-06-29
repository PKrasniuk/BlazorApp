using System;
using System.Threading.Tasks;
using BlazorApp.CommonUI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;

namespace BlazorApp.CommonUI.SharedModels.Components;

public class BreadcrumbsModel : ComponentBase, IDisposable
{
    [Inject] private AppState AppState { get; set; }

    [Inject] private NavigationManager NavigationManager { get; set; }

    [Parameter] public string RootLinkTitle { get; set; }

    [CascadingParameter] private Task<AuthenticationState> AuthenticationStateTask { get; set; }

    protected string[] Paths { get; set; }

    protected string BaseUrl { get; set; }

    private bool IsLoggedIn { get; set; }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanges;
    }

    protected override async Task OnParametersSetAsync()
    {
        IsLoggedIn = false;
        var user = (await AuthenticationStateTask).User;
        if (user.Identity.IsAuthenticated)
            IsLoggedIn = true;
    }

    protected override async Task OnInitializedAsync()
    {
        BaseUrl = NavigationManager.BaseUri;
        await BuildBreadcrumbsAsync();
        NavigationManager.LocationChanged += OnLocationChanges;
        await base.OnInitializedAsync();
    }

    private async void OnLocationChanges(object sender, LocationChangedEventArgs e)
    {
        await BuildBreadcrumbsAsync();
    }

    private async Task BuildBreadcrumbsAsync()
    {
        var uri = NavigationManager.Uri.Replace(BaseUrl, "").Trim();

        if (IsLoggedIn)
            await AppState.SaveLastVisitedUri(uri);

        Paths = string.IsNullOrEmpty(uri) ? new string[] { } : uri.Split('/');
        StateHasChanged();
    }
}