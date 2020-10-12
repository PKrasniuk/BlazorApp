using BlazorApp.Common.Constants;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorApp.CommonUI
{
    public class AppModel : ComponentBase
    {
        [Inject] private HttpClient HttpClient { get; set; }

        [Inject] private NavigationManager NavigationManager { get; set; }

#if !ClientSideBlazor
        [Inject] private IHttpContextAccessor Http { get; set; }
#endif

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            HttpClient.BaseAddress = new Uri(NavigationManager.BaseUri);

#if !ClientSideBlazor
            if (Http?.HttpContext != null && Http.HttpContext.Request.Cookies.Any())
            {
                HttpClient.DefaultRequestHeaders.Add(CommonConstants.CookieName, string.Join(';',
                    Http.HttpContext.Request.Cookies.Select(cookie => $"{cookie.Key}={cookie.Value}").ToList()));
            }
#endif
        }
    }
}