using Microsoft.AspNetCore.Authorization;

namespace BlazorApp.Common.Models.Security;

public class DomainRequirement : IAuthorizationRequirement
{
    public DomainRequirement(string requiredDomain)
    {
        RequiredDomain = requiredDomain;
    }

    public string RequiredDomain { get; }
}