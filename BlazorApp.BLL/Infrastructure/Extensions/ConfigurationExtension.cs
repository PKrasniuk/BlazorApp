using System;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using BlazorApp.Common.Interfaces;
using BlazorApp.Common.Models;
using BlazorApp.Common.Models.EmailModels;
using BlazorApp.Common.Models.Session;
using BlazorApp.Common.Models.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorApp.BLL.Infrastructure.Extensions;

public static partial class ConfigurationExtension
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services,
        IConfiguration configuration, Assembly assembly)
    {
        services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });

        services.AddSameSiteCookiePolicy();

        services.ConfigureApplicationCookie(options =>
        {
            options.Events = new CookieAuthenticationEvents
            {
                OnRedirectToAccessDenied = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;

                    return Task.CompletedTask;
                },
                OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }
            };
        });

        services.AddControllers().AddNewtonsoftJson();
        services.AddSignalR();

        services.AddSwaggerDocument(config =>
        {
            config.PostProcess = document =>
            {
                document.Info.Version = assembly.GetName().Version.ToString();
                document.Info.Title = "Blazor App";
                document.Info.Description = "Blazor App Api";
            };
        });

        services.AddScoped<IUserSession<Guid>, UserSession<Guid>>();

        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddSingleton<IEmailConfiguration>(configuration.GetSection("EmailConfiguration")
            .Get<EmailConfiguration>());

        var autoMapperConfig = new MapperConfiguration(config => { config.AddProfile(new MappingProfile()); });
        services.AddSingleton(autoMapperConfig.CreateMapper());

        services.AddTransient<IValidator<ConfirmEmailModel>, ConfirmEmailModelValidator>();
        services.AddTransient<IValidator<ForgotPasswordModel>, ForgotPasswordModelValidator>();
        services.AddTransient<IValidator<LoginModel>, LoginModelValidator>();
        services.AddTransient<IValidator<RegisterModel>, RegisterModelValidator>();
        services.AddTransient<IValidator<ResetPasswordModel>, ResetPasswordModelValidator>();
        services.AddTransient<IValidator<UserInfoModel>, UserInfoModelValidator>();
        services.AddTransient<IValidator<UserProfileModel>, UserProfileModelValidator>();
        services.AddTransient<IValidator<ApiLogItemModel>, ApiLogItemModelValidator>();
        services.AddTransient<IValidator<TodoModel>, TodoModelValidator>();
        services.AddTransient<IValidator<MessageModel>, MessageModelValidator>();
        services.AddTransient<IValidator<RoleModel>, RoleModelValidator>();

        return services;
    }
}