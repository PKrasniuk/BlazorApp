﻿using Microsoft.AspNetCore.Authorization;

namespace BlazorApp.Common.Models.Security;

public static class Policies
{
    public const string IsAdmin = "IsAdmin";
    public const string IsUser = "IsUser";
    public const string IsReadOnly = "IsReadOnly";
    public const string IsMyDomain = "IsMyDomain";

    public static AuthorizationPolicy IsAdminPolicy()
    {
        return new AuthorizationPolicyBuilder().RequireAuthenticatedUser().RequireClaim("IsAdministrator").Build();
    }

    public static AuthorizationPolicy IsUserPolicy()
    {
        return new AuthorizationPolicyBuilder().RequireAuthenticatedUser().RequireClaim("IsUser").Build();
    }

    public static AuthorizationPolicy IsReadOnlyPolicy()
    {
        return new AuthorizationPolicyBuilder().RequireAuthenticatedUser().RequireClaim("ReadOnly", "true").Build();
    }

    public static AuthorizationPolicy IsMyDomainPolicy()
    {
        return new AuthorizationPolicyBuilder().RequireAuthenticatedUser()
            .AddRequirements(new DomainRequirement("localhost")).Build();
    }
}