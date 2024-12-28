using Microsoft.Extensions.DependencyInjection;
#if !ClientSideBlazor
using System.Linq;
using System.Net.Http;
using BlazorApp.CommonUI.Services;
using BlazorApp.CommonUI.Services.Contracts;
using BlazorApp.CommonUI.Services.Implementations;
using MatBlazor;
using Microsoft.AspNetCore.Components.Authorization;
#endif

namespace BlazorApp.Infrastructure.Extensions;

public static class ServicesExtension
{
    public static IServiceCollection AddAppServicesConfiguration(this IServiceCollection services)
    {
#if !ClientSideBlazor
        services.AddScoped<IAuthorizeApi, AuthorizeApi>();
        services.AddScoped<IUserProfileApi, UserProfileApi>();
        services.AddScoped<AppState>();
        services.AddMatToaster(config =>
        {
            config.Position = MatToastPosition.BottomRight;
            config.PreventDuplicates = true;
            config.NewestOnTop = true;
            config.ShowCloseButton = true;
            config.MaximumOpacity = 95;
            config.VisibleStateDuration = 3000;
        });

        services.AddScoped<HttpClient>();

        services.AddRazorPages();
        services.AddServerSideBlazor();

        var serviceDescriptor = services.FirstOrDefault(descriptor =>
            descriptor.ServiceType == typeof(AuthenticationStateProvider));
        if (serviceDescriptor != null) services.Remove(serviceDescriptor);

        services.AddScoped<AuthenticationStateProvider, IdentityAuthenticationStateProvider>();
#endif
        return services;
    }
}