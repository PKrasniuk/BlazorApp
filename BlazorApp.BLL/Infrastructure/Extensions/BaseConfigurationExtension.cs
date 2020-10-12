using BlazorApp.BLL.Infrastructure.Authorization;
using BlazorApp.BLL.Infrastructure.Handlers;
using BlazorApp.Common.Constants;
using BlazorApp.Common.Models.Security;
using BlazorApp.DAL;
using BlazorApp.DAL.Interfaces;
using BlazorApp.Domain.Entities;
using IdentityServer4.AccessTokenValidation;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace BlazorApp.BLL.Infrastructure.Extensions
{
    public static partial class ConfigurationExtension
    {
        public static IServiceCollection AddBaseConfiguration(this IServiceCollection services,
            IConfiguration configuration, IWebHostEnvironment environment)
        {
            var authAuthority = configuration["BlazorApp:IS4ApplicationUrl"].TrimEnd('/');

            void DbContextOptionsBuilder(DbContextOptionsBuilder builder)
            {
                builder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                    options =>
                    {
                        options.MigrationsAssembly("BlazorApp.DAL");
                        options.SetPostgresVersion(new Version(9, 6));
                    });
            }

            services
                .AddDbContext<ApplicationDbContext>(DbContextOptionsBuilder, ServiceLifetime.Transient)
                .AddDbContext<ConfigurationDbContext>(DbContextOptionsBuilder, ServiceLifetime.Transient)
                .AddDbContext<PersistedGrantDbContext>(DbContextOptionsBuilder, ServiceLifetime.Transient);

            services.AddIdentity<ApplicationUser<Guid>, IdentityRole<Guid>>()
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services
                .AddScoped<IUserClaimsPrincipalFactory<ApplicationUser<Guid>>, AdditionalUserClaimsPrincipalFactory>();

            var identityServerBuilder = services.AddIdentityServer(options =>
                {
                    options.IssuerUri = authAuthority;
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                })
                .AddConfigurationStore(options => { options.ConfigureDbContext = DbContextOptionsBuilder; })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = DbContextOptionsBuilder;
                    options.EnableTokenCleanup = true;
                    options.TokenCleanupInterval = 3600;
                })
                .AddAspNetIdentity<ApplicationUser<Guid>>();

            if (environment.IsDevelopment()) identityServerBuilder.AddDeveloperSigningCredential();

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = IdentityServerAuthenticationDefaults.AuthenticationScheme;
                })
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = authAuthority;
                    options.SupportedTokens = SupportedTokens.Jwt;
                    options.RequireHttpsMetadata = environment.IsProduction();
                    options.ApiName = CommonConstants.ApiName;
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.IsAdmin, Policies.IsAdminPolicy());
                options.AddPolicy(Policies.IsUser, Policies.IsUserPolicy());
                options.AddPolicy(Policies.IsReadOnly, Policies.IsReadOnlyPolicy());
                options.AddPolicy(Policies.IsMyDomain, Policies.IsMyDomainPolicy());
            });

            services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
            services.AddTransient<IAuthorizationHandler, DomainRequirementHandler>();
            services.AddTransient<IAuthorizationHandler, PermissionRequirementHandler>();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                //options.Password.RequiredUniqueChars = 6;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;

                if (Convert.ToBoolean(configuration["BlazorApp:RequireConfirmedEmail"] ?? "false"))
                {
                    options.User.RequireUniqueEmail = false;
                    options.SignIn.RequireConfirmedEmail = true;
                }
            });

            services.AddTransient<IDatabaseInitializer, ApplicationDbInitializer>();

            services.AddTransient(s => s.GetRequiredService<ApplicationDbContext>() as IApplicationDbContext);

            return services;
        }
    }
}