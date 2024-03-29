﻿using Microsoft.AspNetCore.Authorization;

namespace BlazorApp.BLL.Infrastructure.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }

    public string Permission { get; set; }
}