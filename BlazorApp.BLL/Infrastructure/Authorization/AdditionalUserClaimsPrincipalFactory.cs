using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BlazorApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BlazorApp.BLL.Infrastructure.Authorization;

public class
    AdditionalUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser<Guid>, IdentityRole<Guid>>
{
    public AdditionalUserClaimsPrincipalFactory(UserManager<ApplicationUser<Guid>> userManager,
        RoleManager<IdentityRole<Guid>> roleManager, IOptions<IdentityOptions> options) : base(userManager,
        roleManager, options)
    {
    }

    public override async Task<ClaimsPrincipal> CreateAsync(ApplicationUser<Guid> user)
    {
        var principal = await base.CreateAsync(user);

        if (!string.IsNullOrWhiteSpace(user.FirstName))
            ((ClaimsIdentity)principal.Identity).AddClaims(new[]
                { new Claim(ClaimTypes.GivenName, user.FirstName) });

        if (!string.IsNullOrWhiteSpace(user.LastName))
            ((ClaimsIdentity)principal.Identity).AddClaims(new[] { new Claim(ClaimTypes.Surname, user.LastName) });

        if (!string.IsNullOrWhiteSpace(user.Email))
            ((ClaimsIdentity)principal.Identity).AddClaims(new[] { new Claim(ClaimTypes.Email, user.Email) });

        return principal;
    }
}