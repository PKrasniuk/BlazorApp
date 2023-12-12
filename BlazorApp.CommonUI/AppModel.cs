using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.CommonUI;

public class AppModel : ComponentBase
{
    [Inject] private HttpClient HttpClient { get; set; }

    [Inject] private NavigationManager NavigationManager { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        HttpClient.BaseAddress = new Uri(NavigationManager.BaseUri);
    }
}