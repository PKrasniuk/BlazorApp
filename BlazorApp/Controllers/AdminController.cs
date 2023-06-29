using System.Threading.Tasks;
using BlazorApp.BLL.Interfaces;
using BlazorApp.Common.Models;
using BlazorApp.Common.Models.Security;
using BlazorApp.Common.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazorApp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly IAccountManager _accountManager;
    private readonly IRoleManager _roleManager;

    public AdminController(IAccountManager accountManager, IRoleManager roleManager)
    {
        _accountManager = accountManager;
        _roleManager = roleManager;
    }

    [HttpGet("Users")]
    [Authorize]
    public async Task<ApiResponse> GetUsersAsync([FromQuery] int pageSize = 10, [FromQuery] int pageNumber = 0)
    {
        return await _accountManager.GetUsersAsync(pageSize, pageNumber);
    }

    [HttpGet("Permissions")]
    [Authorize]
    public async Task<ApiResponse> GetPermissionsAsync()
    {
        return await _roleManager.GetPermissionsAsync();
    }

    [HttpGet("Roles")]
    [Authorize(Permissions.Role.Read)]
    public async Task<ApiResponse> GetRolesAsync([FromQuery] int pageSize = 10, [FromQuery] int pageNumber = 0)
    {
        return await _roleManager.GetRolesAsync(pageSize, pageNumber);
    }

    [HttpGet("Role/{roleName}")]
    [Authorize]
    public async Task<ApiResponse> GetRoleAsync(string roleName)
    {
        return await _roleManager.GetRoleAsync(roleName);
    }

    [HttpPost("Role")]
    [Authorize(Permissions.Role.Create)]
    public async Task<ApiResponse> CreateRoleAsync([FromBody] RoleModel roleModel)
    {
        return await _roleManager.CreateRoleAsync(roleModel);
    }

    [HttpPut("Role")]
    [Authorize(Permissions.Role.Update)]
    public async Task<ApiResponse> UpdateRoleAsync([FromBody] RoleModel roleModel)
    {
        return await _roleManager.UpdateRoleAsync(roleModel);
    }

    [HttpDelete("Role/{roleName}")]
    [Authorize(Permissions.Role.Delete)]
    public async Task<ApiResponse> DeleteRoleAsync(string roleName)
    {
        return await _roleManager.DeleteRoleAsync(roleName);
    }
}