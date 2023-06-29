using BlazorApp.BLL.Interfaces;
using BlazorApp.BLL.Managers;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorApp.BLL.Infrastructure.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddManagersCollection(this IServiceCollection services)
    {
        services.AddTransient<IEmailManager, EmailManager>();

        services.AddTransient<IApiLogManager, ApiLogManager>();
        services.AddTransient<IAccountManager, AccountManager>();
        services.AddTransient<IMessageManager, MessageManager>();
        services.AddTransient<IRoleManager, RoleManager>();
        services.AddTransient<ITodoManager, TodoManager>();
        services.AddTransient<IUserProfileManager, UserProfileManager>();
        services.AddTransient<IDbLogManager, DbLogManager>();

        return services;
    }
}