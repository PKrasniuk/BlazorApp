using BlazorApp.Common.Models.Security;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazorApp.BLL.Infrastructure.Handlers
{
    public class DomainRequirementHandler : AuthorizationHandler<DomainRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            DomainRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == ClaimTypes.Email)) return Task.CompletedTask;

            var emailAddress = context.User.FindFirst(c => c.Type == ClaimTypes.Email).Value;
            if (emailAddress.ToLower().EndsWith(requirement.RequiredDomain.ToLower())) context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}