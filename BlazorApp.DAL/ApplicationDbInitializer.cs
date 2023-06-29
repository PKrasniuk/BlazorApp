using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BlazorApp.Common.Constants;
using BlazorApp.Common.Models.Security;
using BlazorApp.DAL.Infrastructure.Configurations;
using BlazorApp.DAL.Interfaces;
using BlazorApp.Domain.Entities;
using IdentityModel;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlazorApp.DAL;

public class ApplicationDbInitializer : IDatabaseInitializer
{
    private readonly ConfigurationDbContext _configurationContext;
    private readonly ApplicationDbContext _context;
    private readonly ILogger _logger;
    private readonly PersistedGrantDbContext _persistedGrantContext;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly UserManager<ApplicationUser<Guid>> _userManager;

    public ApplicationDbInitializer(
        ApplicationDbContext context,
        PersistedGrantDbContext persistedGrantContext,
        ConfigurationDbContext configurationContext,
        ILogger<ApplicationDbInitializer> logger,
        UserManager<ApplicationUser<Guid>> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _persistedGrantContext = persistedGrantContext;
        _configurationContext = configurationContext;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await MigrateAsync();

        await SeedAspIdentityCoreAsync();

        await SeedIdentityServerAsync();

        await SeedBlazorAppAsync();
    }

    private async Task MigrateAsync()
    {
        await _context.Database.MigrateAsync().ConfigureAwait(false);
        await _persistedGrantContext.Database.MigrateAsync().ConfigureAwait(false);
        await _configurationContext.Database.MigrateAsync().ConfigureAwait(false);
    }

    private async Task SeedAspIdentityCoreAsync()
    {
        if (!await _context.Users.AnyAsync())
        {
            await EnsureRoleAsync(RoleConstants.AdminRoleName, ApplicationPermissions.GetAllPermissionValues());
            await EnsureRoleAsync(RoleConstants.UserRoleName, new string[] { });

            await CreateUserAsync("admin", "admin123", "Admin", "Admin Blazor", "Blazor",
                "krasniuk@ukr.net", "+1 (123) 456-7890", new[] { RoleConstants.AdminRoleName });
            await CreateUserAsync("user", "user123", "User", "User Blazor", "Blazor", "pkrasniuk@outlook.com",
                "+1 (123) 456-7890`", new[] { RoleConstants.UserRoleName });

            _logger.LogInformation("Inbuilt account generation completed");
        }
        else
        {
            var adminRole = await _roleManager.FindByNameAsync(RoleConstants.AdminRoleName);
            var allClaims = ApplicationPermissions.GetAllPermissionValues().Distinct().ToList();
            var roleClaims = (await _roleManager.GetClaimsAsync(adminRole)).Select(c => c.Value).ToList();
            var newClaims = allClaims.Except(roleClaims);
            foreach (var claim in newClaims)
                await _roleManager.AddClaimAsync(adminRole, new Claim(ClaimConstants.Permission, claim));
            var deprecatedClaims = roleClaims.Except(allClaims);
            var roles = await _roleManager.Roles.ToListAsync();
            foreach (var claim in deprecatedClaims)
            foreach (var role in roles)
                await _roleManager.RemoveClaimAsync(role, new Claim(ClaimConstants.Permission, claim));
        }
    }

    private async Task SeedIdentityServerAsync()
    {
        if (!await _configurationContext.Clients.AnyAsync())
        {
            _logger.LogInformation("Seeding IdentityServer Clients");
            foreach (var client in IdentityServerConfiguration.GetClients())
                await _configurationContext.Clients.AddAsync(client.ToEntity());

            await _configurationContext.SaveChangesAsync();
        }

        if (!await _configurationContext.IdentityResources.AnyAsync())
        {
            _logger.LogInformation("Seeding IdentityServer Identity Resources");
            foreach (var resource in IdentityServerConfiguration.GetIdentityResources())
                await _configurationContext.IdentityResources.AddAsync(resource.ToEntity());

            await _configurationContext.SaveChangesAsync();
        }

        if (!await _configurationContext.ApiResources.AnyAsync())
        {
            _logger.LogInformation("Seeding IdentityServer API Resources");
            foreach (var resource in IdentityServerConfiguration.GetApiResources())
                await _configurationContext.ApiResources.AddAsync(resource.ToEntity());

            await _configurationContext.SaveChangesAsync();
        }
    }

    private async Task SeedBlazorAppAsync()
    {
        var adminUser = await _userManager.FindByNameAsync("admin");
        var user = await _userManager.FindByNameAsync("user");

        if (!_context.UserProfiles.Any())
            await _context.UserProfiles.AddRangeAsync(new UserProfile<Guid>
            {
                UserId = adminUser.Id,
                Count = 2,
                IsNavOpen = true,
                LastPageVisited = CommonConstants.DefaultPageVisited,
                IsNavMinified = false,
                LastUpdatedDate = DateTime.Now
            }, new UserProfile<Guid>
            {
                UserId = user.Id,
                Count = 2,
                IsNavOpen = true,
                LastPageVisited = CommonConstants.DefaultPageVisited,
                IsNavMinified = false,
                LastUpdatedDate = DateTime.Now
            });

        if (!_context.Todos.Any())
            await _context.Todos.AddRangeAsync(
                new Todo<Guid>
                {
                    IsCompleted = false,
                    Title = "Test Blazor App"
                },
                new Todo<Guid>
                {
                    IsCompleted = false,
                    Title = "New Test Blazor App"
                }
            );

        if (!_context.ApiLogs.Any())
            await _context.ApiLogs.AddRangeAsync(
                new ApiLogItem<Guid>
                {
                    RequestTime = DateTime.Now,
                    ResponseMillis = 30,
                    StatusCode = 200,
                    Method = "Get",
                    Path = "/api/seed",
                    QueryString = "",
                    RequestBody = "",
                    ResponseBody = "",
                    IpAddress = "::1",
                    ApplicationUserId = adminUser.Id
                },
                new ApiLogItem<Guid>
                {
                    RequestTime = DateTime.Now,
                    ResponseMillis = 30,
                    StatusCode = 200,
                    Method = "Get",
                    Path = "/api/seed",
                    QueryString = "",
                    RequestBody = "",
                    ResponseBody = "",
                    IpAddress = "::1",
                    ApplicationUserId = user.Id
                }
            );

        await _context.SaveChangesAsync();
    }

    private async Task EnsureRoleAsync(string roleName, string[] claims)
    {
        if (await _roleManager.FindByNameAsync(roleName) == null)
        {
            if (claims == null) claims = new string[] { };

            var invalidClaims = claims.Where(c => ApplicationPermissions.GetPermissionByValue(c) == null).ToArray();
            if (invalidClaims.Any())
                throw new Exception("The following claim types are invalid: " + string.Join(", ", invalidClaims));

            var applicationRole = new IdentityRole<Guid>(roleName);
            await _roleManager.CreateAsync(applicationRole);

            var role = await _roleManager.FindByNameAsync(applicationRole.Name);

            foreach (var claim in claims.Distinct())
            {
                var result = await _roleManager.AddClaimAsync(role,
                    new Claim(ClaimConstants.Permission, ApplicationPermissions.GetPermissionByValue(claim)));
                if (!result.Succeeded) await _roleManager.DeleteAsync(role);
            }
        }
    }

    private async Task<ApplicationUser<Guid>> CreateUserAsync(string userName, string password, string firstName,
        string fullName, string lastName, string email, string phoneNumber, string[] roles)
    {
        var applicationUser = _userManager.FindByNameAsync(userName).Result;

        if (applicationUser == null)
        {
            applicationUser = new ApplicationUser<Guid>
            {
                UserName = userName,
                Email = email,
                PhoneNumber = phoneNumber,
                FullName = fullName,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(applicationUser, password);
            if (!result.Succeeded) throw new Exception(result.Errors.First().Description);

            await _userManager.AddClaimsAsync(applicationUser, new[]
            {
                new Claim(JwtClaimTypes.Name, userName),
                new Claim(JwtClaimTypes.GivenName, firstName),
                new Claim(JwtClaimTypes.FamilyName, lastName),
                new Claim(JwtClaimTypes.Email, email),
                new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                new Claim(JwtClaimTypes.PhoneNumber, phoneNumber)
            });

            foreach (var role in roles.Distinct())
                await _userManager.AddClaimAsync(applicationUser, new Claim($"Is{role}", "true"));

            var user = await _userManager.FindByNameAsync(applicationUser.UserName);

            try
            {
                result = await _userManager.AddToRolesAsync(user, roles.Distinct());
            }
            catch
            {
                await _userManager.DeleteAsync(user);
                throw;
            }

            if (!result.Succeeded) await _userManager.DeleteAsync(user);
        }

        return applicationUser;
    }
}