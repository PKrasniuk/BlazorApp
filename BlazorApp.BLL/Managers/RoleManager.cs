using BlazorApp.BLL.Interfaces;
using BlazorApp.Common.Constants;
using BlazorApp.Common.Models;
using BlazorApp.Common.Models.Security;
using BlazorApp.Common.Wrappers;
using BlazorApp.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazorApp.BLL.Managers
{
    public class RoleManager : IRoleManager
    {
        private readonly ILogger<RoleManager> _logger;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly UserManager<ApplicationUser<Guid>> _userManager;

        public RoleManager(UserManager<ApplicationUser<Guid>> userManager, RoleManager<IdentityRole<Guid>> roleManager,
            ILogger<RoleManager> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<ApiResponse> GetPermissionsAsync()
        {
            await Task.Delay(1);

            var permissions = ApplicationPermissions.GetAllPermissionNames();
            return new ApiResponse(StatusCodes.Status200OK, "Permissions list fetched", permissions);
        }

        public async Task<ApiResponse> GetRolesAsync(int pageSize, int pageNumber)
        {
            var roleModelList = new List<RoleModel>();

            try
            {
                var roleList = _roleManager.Roles.AsQueryable().OrderBy(x => x.Id).Skip(pageNumber * pageSize)
                    .Take(pageSize).ToList();
                foreach (var role in roleList)
                {
                    var claims = await _roleManager.GetClaimsAsync(role);
                    var permissions = claims.Where(x => x.Type == ClaimConstants.Permission)
                        .Select(x => ApplicationPermissions.GetPermissionByValue(x.Value).Name).ToList();

                    roleModelList.Add(new RoleModel
                    {
                        Name = role.Name,
                        Permissions = permissions
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Get Roles Failed: {0}", ex.Message);
                return new ApiResponse(StatusCodes.Status400BadRequest, $"Get Roles Failed: {ex.Message} ");
            }

            return new ApiResponse(StatusCodes.Status200OK, "Roles list fetched", roleModelList);
        }

        public async Task<ApiResponse> GetRoleAsync(string roleName)
        {
            RoleModel roleModel;

            try
            {
                var identityRole = await _roleManager.FindByNameAsync(roleName);
                var claims = await _roleManager.GetClaimsAsync(identityRole);
                var permissions = claims.Where(x => x.Type == ClaimConstants.Permission)
                    .Select(x => ApplicationPermissions.GetPermissionByValue(x.Value).Name).ToList();

                roleModel = new RoleModel
                {
                    Name = roleName,
                    Permissions = permissions
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Get Role Failed: {0}", ex.Message);
                return new ApiResponse(StatusCodes.Status400BadRequest, $"Get Role Failed: {ex.Message} ");
            }

            return new ApiResponse(StatusCodes.Status200OK, "Role fetched", roleModel);
        }

        public async Task<ApiResponse> CreateRoleAsync(RoleModel roleModel)
        {
            try
            {
                if (_roleManager.Roles.Any(r => r.Name == roleModel.Name))
                    return new ApiResponse(StatusCodes.Status400BadRequest, "Role already exists");

                var result = await _roleManager.CreateAsync(new IdentityRole<Guid>(roleModel.Name));
                if (!result.Succeeded)
                {
                    var errorMessage = result.Errors.Select(x => x.Description).Aggregate((i, j) => i + " - " + j);
                    _logger.LogError("Role Creation Failed: {0}", errorMessage);
                    return new ApiResponse(StatusCodes.Status500InternalServerError, errorMessage);
                }

                var role = await _roleManager.FindByNameAsync(roleModel.Name);

                foreach (var claim in roleModel.Permissions)
                {
                    var resultAddClaim = await _roleManager.AddClaimAsync(role,
                        new Claim(ClaimConstants.Permission, ApplicationPermissions.GetPermissionByName(claim)));
                    if (!resultAddClaim.Succeeded) await _roleManager.DeleteAsync(role);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Role Creation Failed: {0}", ex.Message);
                return new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message);
            }

            return new ApiResponse(StatusCodes.Status200OK);
        }

        public async Task<ApiResponse> UpdateRoleAsync(RoleModel roleModel)
        {
            try
            {
                if (!_roleManager.Roles.Any(r => r.Name == roleModel.Name))
                    return new ApiResponse(StatusCodes.Status400BadRequest, "This role doesn't exists");

                var identityRole = await _roleManager.FindByNameAsync(roleModel.Name);
                var claims = await _roleManager.GetClaimsAsync(identityRole);
                var permissions = claims.Where(x => x.Type == ClaimConstants.Permission).Select(x => x.Value).ToList();

                foreach (var permission in permissions)
                    await _roleManager.RemoveClaimAsync(identityRole, new Claim(ClaimConstants.Permission, permission));

                foreach (var claim in roleModel.Permissions)
                {
                    var result = await _roleManager.AddClaimAsync(identityRole,
                        new Claim(ClaimConstants.Permission, ApplicationPermissions.GetPermissionByName(claim)));
                    if (!result.Succeeded) await _roleManager.DeleteAsync(identityRole);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Role Updating Failed: {0}", ex.Message);
                return new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message);
            }

            return new ApiResponse(StatusCodes.Status200OK);
        }

        public async Task<ApiResponse> DeleteRoleAsync(string roleName)
        {
            try
            {
                var users = await _userManager.GetUsersInRoleAsync(roleName);
                if (users.Any())
                    return new ApiResponse(StatusCodes.Status404NotFound,
                        "This role is still used by a user, you cannot delete it");

                var role = await _roleManager.FindByNameAsync(roleName);
                await _roleManager.DeleteAsync(role);

                return new ApiResponse(StatusCodes.Status200OK, "Role Deletion Successful");
            }
            catch (Exception ex)
            {
                _logger.LogError("Role Deletion Failed: {0}", ex.Message);
                return new ApiResponse(StatusCodes.Status400BadRequest, "Role Deletion Failed");
            }
        }
    }
}