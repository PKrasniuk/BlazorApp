using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BlazorApp.BLL.Infrastructure.Exceptions;
using BlazorApp.BLL.Infrastructure.Helpers;
using BlazorApp.BLL.Interfaces;
using BlazorApp.Common.Constants;
using BlazorApp.Common.Models;
using BlazorApp.Common.Models.EmailModels;
using BlazorApp.Common.Models.Security;
using BlazorApp.Common.Wrappers;
using BlazorApp.DAL.Interfaces;
using BlazorApp.Domain.Entities;
using Duende.IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BlazorApp.BLL.Managers;

public class AccountManager : IAccountManager
{
    private readonly IConfiguration _configuration;
    private readonly IApplicationDbContext _db;
    private readonly IEmailManager _emailManager;
    private readonly ILogger<AccountManager> _logger;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly SignInManager<ApplicationUser<Guid>> _signInManager;
    private readonly UserManager<ApplicationUser<Guid>> _userManager;
    private readonly IUserProfileManager _userProfileManager;

    public AccountManager(UserManager<ApplicationUser<Guid>> userManager,
        SignInManager<ApplicationUser<Guid>> signInManager, RoleManager<IdentityRole<Guid>> roleManager,
        IUserProfileManager userProfileManager, ILogger<AccountManager> logger, IEmailManager emailManager,
        IConfiguration configuration, IApplicationDbContext db)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _userProfileManager = userProfileManager;
        _emailManager = emailManager;
        _logger = logger;
        _configuration = configuration;
        _db = db;
    }

    public async Task<ApiResponse> LoginAsync(LoginModel model)
    {
        try
        {
            var result =
                await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, true);

            if (result.IsLockedOut)
            {
                _logger.LogInformation("User Locked out: {0}", model.UserName);
                return new ApiResponse(StatusCodes.Status401Unauthorized, "User is locked out!");
            }

            if (result.IsNotAllowed)
            {
                _logger.LogInformation("User not allowed to log in: {0}", model.UserName);
                return new ApiResponse(StatusCodes.Status401Unauthorized, "Login not allowed!");
            }

            if (result.Succeeded)
            {
                _logger.LogInformation("Logged In: {0}", model.UserName);
                return await _userProfileManager.GetLastPageVisitedAsync(model.UserName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInformation("Login Failed: " + ex.Message);
        }

        _logger.LogInformation("Invalid Password for user {0}}", model.UserName);
        return new ApiResponse(StatusCodes.Status401Unauthorized, "Login Failed");
    }

    public async Task<ApiResponse> RegisterAsync(RegisterModel model)
    {
        try
        {
            var requireConfirmEmail =
                Convert.ToBoolean(_configuration["BlazorApp:RequireConfirmedEmail"] ?? "false");
            await RegisterNewUserAsync(model.UserName, model.Email, model.Password, requireConfirmEmail);

            if (requireConfirmEmail) return new ApiResponse(StatusCodes.Status200OK, "Register User Success");

            return await LoginAsync(new LoginModel
            {
                UserName = model.UserName,
                Password = model.Password
            });
        }
        catch (DomainException ex)
        {
            _logger.LogError("Register User Failed: {0}, {1}", ex.Description, ex.Message);
            return new ApiResponse(StatusCodes.Status400BadRequest, $"Register User Failed: {ex.Description} ");
        }
        catch (Exception ex)
        {
            _logger.LogError("Register User Failed: {0}", ex.Message);
            return new ApiResponse(StatusCodes.Status400BadRequest, "Register User Failed");
        }
    }

    public async Task<ApiResponse> ConfirmEmailAsync(ConfirmEmailModel model)
    {
        if (string.IsNullOrEmpty(model.UserId) || model.Token == null)
            return new ApiResponse(StatusCodes.Status404NotFound, "User does not exist");

        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null)
        {
            _logger.LogInformation("User does not exist: {0}", model.UserId);
            return new ApiResponse(StatusCodes.Status404NotFound, "User does not exist");
        }

        var result = await _userManager.ConfirmEmailAsync(user, model.Token);
        if (!result.Succeeded)
        {
            _logger.LogInformation("User Email Confirmation Failed: {0}",
                string.Join(",", result.Errors.Select(i => i.Description)));
            return new ApiResponse(StatusCodes.Status400BadRequest, "User Email Confirmation Failed");
        }

        await _signInManager.SignInAsync(user, true);

        return new ApiResponse(StatusCodes.Status200OK, "Success");
    }

    public async Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
        {
            _logger.LogInformation("Forgot Password with non-existent email / user: {0}", model.Email);
            return new ApiResponse(StatusCodes.Status200OK, "Success");
        }

        try
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl =
                $"{_configuration["BlazorApp:ApplicationUrl"]}/Account/ResetPassword/{user.Id}?token={token}";

            var email = new EmailMessageModel();
            email.ToAddresses.Add(new EmailAddressModel(user.Email, user.Email));
            email.BuildForgotPasswordEmail(user.UserName, callbackUrl, token);

            _logger.LogInformation("Forgot Password Email Sent: {0}", user.Email);
            await _emailManager.SendEmailAsync(email);
            return new ApiResponse(StatusCodes.Status200OK, "Forgot Password Email Sent");
        }
        catch (Exception ex)
        {
            _logger.LogInformation("Forgot Password email failed: {0}", ex.Message);
        }

        return new ApiResponse(StatusCodes.Status200OK, "Success");
    }

    public async Task<ApiResponse> ResetPasswordAsync(ResetPasswordModel model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null)
        {
            _logger.LogInformation("User does not exist: {0}", model.UserId);
            return new ApiResponse(StatusCodes.Status404NotFound, "User does not exist");
        }

        try
        {
            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                var email = new EmailMessageModel();
                email.ToAddresses.Add(new EmailAddressModel(user.Email, user.Email));
                email.BuildPasswordResetEmail(user.UserName);

                _logger.LogInformation("Reset Password Successful Email Sent: {0}", user.Email);
                await _emailManager.SendEmailAsync(email);

                return new ApiResponse(StatusCodes.Status200OK,
                    $"Reset Password Successful Email Sent: {user.Email}");
            }

            _logger.LogInformation("Error while resetting the password!: {0}", user.UserName);
            return new ApiResponse(StatusCodes.Status400BadRequest,
                $"Error while resetting the password!: {user.UserName}");
        }
        catch (Exception ex)
        {
            _logger.LogInformation("Reset Password failed: {0}", ex.Message);
            return new ApiResponse(StatusCodes.Status400BadRequest,
                $"Error while resetting the password!: {ex.Message}");
        }
    }

    public async Task<ApiResponse> UserInfoAsync(ClaimsPrincipal userClaimsPrincipal)
    {
        return new ApiResponse(StatusCodes.Status200OK, "Retrieved UserInfo",
            await BuildUserInfoModelAsync(userClaimsPrincipal));
    }

    public async Task<ApiResponse> UpdateUserAsync(UserInfoModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null)
        {
            _logger.LogInformation("User does not exist: {0}", model.Email);
            return new ApiResponse(StatusCodes.Status404NotFound, "User does not exist");
        }

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Email = model.Email;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            _logger.LogInformation("User Update Failed: {0}",
                string.Join(",", result.Errors.Select(i => i.Description)));
            return new ApiResponse(StatusCodes.Status400BadRequest, "User Update Failed");
        }

        return new ApiResponse(StatusCodes.Status200OK, "User Updated Successfully");
    }

    public async Task<ApiResponse> CreateAsync(RegisterModel model)
    {
        try
        {
            var user = new ApplicationUser<Guid>
            {
                UserName = model.UserName,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return new ApiResponse(StatusCodes.Status400BadRequest,
                    "Register User Failed: " + string.Join(",", result.Errors.Select(i => i.Description)));

            await _userManager.AddClaimsAsync(user, new[]
            {
                new Claim(Policies.IsUser, string.Empty),
                new Claim(JwtClaimTypes.Name, model.UserName),
                new Claim(JwtClaimTypes.Email, model.Email),
                new Claim(JwtClaimTypes.EmailVerified, "false", ClaimValueTypes.Boolean)
            });

            await _userManager.AddToRoleAsync(user, RoleConstants.UserRoleName);

            if (Convert.ToBoolean(_configuration["BlazorApp:RequireConfirmedEmail"] ?? "false"))
            {
                try
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl =
                        $"{_configuration["BlazorApp:ApplicationUrl"]}/Account/ConfirmEmail/{user.Id}?token={token}";

                    var email = new EmailMessageModel();
                    email.ToAddresses.Add(new EmailAddressModel(user.Email, user.Email));
                    email = email.BuildNewUserConfirmationEmail(user.UserName, user.Email, callbackUrl,
                        user.Id.ToString(), token);

                    _logger.LogInformation("New user created: {0}", user);
                    await _emailManager.SendEmailAsync(email);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation("New user email failed: {0}", ex.Message);
                }

                return new ApiResponse(StatusCodes.Status200OK, "Create User Success");
            }

            try
            {
                var email = new EmailMessageModel();
                email.ToAddresses.Add(new EmailAddressModel(user.Email, user.Email));
                email.BuildNewUserEmail(user.FullName, user.UserName, user.Email, model.Password);

                _logger.LogInformation("New user created: {0}", user);
                await _emailManager.SendEmailAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("New user email failed: {0}", ex.Message);
            }

            var userInfo = new UserInfoModel
            {
                UserId = user.Id.ToString(),
                IsAuthenticated = false,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = new List<string> { RoleConstants.UserRoleName }
            };

            return new ApiResponse(StatusCodes.Status200OK, "Created New User", userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError("Create User Failed: {0}", ex.Message);
            return new ApiResponse(StatusCodes.Status400BadRequest, "Create User Failed");
        }
    }

    public async Task<ApiResponse> DeleteAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user.Id == Guid.Empty) return new ApiResponse(StatusCodes.Status404NotFound, "User does not exist");

        try
        {
            var apiLogs = await _db.ApiLogs.Where(a => a.ApplicationUserId == user.Id).ToArrayAsync();
            _db.ApiLogs.RemoveRange(apiLogs);
            await _db.SaveChangesAsync();

            await _userManager.DeleteAsync(user);

            return new ApiResponse(StatusCodes.Status200OK, "User Deletion Successful");
        }
        catch
        {
            return new ApiResponse(StatusCodes.Status400BadRequest, "User Deletion Failed");
        }
    }

    public async Task<ApiResponse> GetUserAsync(ClaimsPrincipal userClaimsPrincipal)
    {
        await Task.Delay(1);

        var userInfoModel = userClaimsPrincipal != null && userClaimsPrincipal.Identity.IsAuthenticated
            ? new UserInfoModel { UserName = userClaimsPrincipal.Identity.Name, IsAuthenticated = true }
            : new UserInfoModel { IsAuthenticated = false, Roles = new List<string>() };
        return new ApiResponse(StatusCodes.Status200OK, "Get User Successful", userInfoModel);
    }

    public async Task<ApiResponse> ListRolesAsync()
    {
        return new ApiResponse(StatusCodes.Status200OK, string.Empty,
            await _roleManager.Roles.Select(x => x.Name).ToListAsync());
    }

    public async Task<ApiResponse> UpdateAsync(UserInfoModel model)
    {
        var appUser = await _userManager.FindByIdAsync(model.UserId).ConfigureAwait(true);
        appUser.UserName = model.UserName;
        appUser.FirstName = model.FirstName;
        appUser.LastName = model.LastName;
        appUser.Email = model.Email;

        try
        {
            await _userManager.UpdateAsync(appUser).ConfigureAwait(true);
        }
        catch
        {
            return new ApiResponse(StatusCodes.Status500InternalServerError, "Error Updating User");
        }

        if (model.Roles != null)
            try
            {
                var currentUserRoles =
                    (List<string>)await _userManager.GetRolesAsync(appUser).ConfigureAwait(true);
                var rolesToAdd = model.Roles.Where(newUserRole => !currentUserRoles.Contains(newUserRole)).ToList();
                await _userManager.AddToRolesAsync(appUser, rolesToAdd).ConfigureAwait(true);

                foreach (var role in rolesToAdd)
                    await _userManager.AddClaimAsync(appUser, new Claim($"Is{role}", "true")).ConfigureAwait(true);

                var rolesToRemove = currentUserRoles.Where(role => !model.Roles.Contains(role)).ToList();
                await _userManager.RemoveFromRolesAsync(appUser, rolesToRemove).ConfigureAwait(true);

                foreach (var role in rolesToRemove)
                    await _userManager.RemoveClaimAsync(appUser, new Claim($"Is{role}", "true"))
                        .ConfigureAwait(true);
            }
            catch
            {
                return new ApiResponse(StatusCodes.Status500InternalServerError, "Error Updating Roles");
            }

        return new ApiResponse(StatusCodes.Status200OK, "User Updated");
    }

    public async Task<ApiResponse> AdminResetUserPasswordAsync(string id, string newPassword,
        ClaimsPrincipal userClaimsPrincipal)
    {
        ApplicationUser<Guid> user;

        try
        {
            user = await _userManager.FindByIdAsync(id);
            if (user.Id == Guid.Empty) throw new KeyNotFoundException();
        }
        catch (KeyNotFoundException ex)
        {
            return new ApiResponse(StatusCodes.Status400BadRequest, "Unable to find user" + ex.Message);
        }

        try
        {
            var passToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, passToken, newPassword);
            if (result.Succeeded)
            {
                _logger.LogInformation(user.UserName + "'s password reset; Requested from Admin interface by:" +
                                       userClaimsPrincipal.Identity.Name);
                return new ApiResponse(StatusCodes.Status204NoContent, user.UserName + " password reset");
            }

            _logger.LogInformation(user.UserName + "'s password reset failed; Requested from Admin interface by:" +
                                   userClaimsPrincipal.Identity.Name);
            if (result.Errors.Any())
                return new ApiResponse(StatusCodes.Status400BadRequest,
                    string.Join(',', result.Errors.Select(x => x.Description)));

            throw new Exception();
        }
        catch (Exception ex)
        {
            _logger.LogInformation(user.UserName + "'s password reset failed; Requested from Admin interface by:" +
                                   userClaimsPrincipal.Identity.Name);
            return new ApiResponse(StatusCodes.Status400BadRequest, ex.Message);
        }
    }

    public async Task<ApiResponse> LogoutAsync()
    {
        await _signInManager.SignOutAsync();
        return new ApiResponse(StatusCodes.Status200OK, "Logout Successful");
    }

    public async Task<ApiResponse> RegisterNewUserAsync(string userName, string email, string password,
        bool requireConfirmEmail)
    {
        var user = new ApplicationUser<Guid>
        {
            UserName = userName,
            Email = email
        };

        var createUserResult = password == null
            ? await _userManager.CreateAsync(user)
            : await _userManager.CreateAsync(user, password);
        if (!createUserResult.Succeeded)
            throw new DomainException(string.Join(",", createUserResult.Errors.Select(i => i.Description)));

        await _userManager.AddClaimsAsync(user, new[]
        {
            new Claim(Policies.IsUser, string.Empty),
            new Claim(JwtClaimTypes.Name, user.UserName),
            new Claim(JwtClaimTypes.Email, user.Email),
            new Claim(JwtClaimTypes.EmailVerified, "false", ClaimValueTypes.Boolean)
        });

        await _userManager.AddToRoleAsync(user, RoleConstants.UserRoleName);

        _logger.LogInformation("New user registered: {0}", user);

        var emailMessage = new EmailMessageModel();

        if (requireConfirmEmail)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl =
                $"{_configuration["BlazorApp:ApplicationUrl"]}/Account/ConfirmEmail/{user.Id}?token={token}";
            emailMessage.BuildNewUserConfirmationEmail(user.UserName, user.Email, callbackUrl, user.Id.ToString(),
                token);
        }
        else
        {
            emailMessage.BuildNewUserEmail(user.FullName, user.UserName, user.Email, password);
        }

        emailMessage.ToAddresses.Add(new EmailAddressModel(user.Email, user.Email));
        try
        {
            await _emailManager.SendEmailAsync(emailMessage);
        }
        catch (Exception ex)
        {
            _logger.LogInformation("New user email failed: Body: {0}, Error: {1}", emailMessage.Body, ex.Message);
        }

        return new ApiResponse(StatusCodes.Status200OK, "New user registered successfully", user);
    }

    public async Task<ApiResponse> GetUsersAsync(int pageSize = 10, int pageNumber = 0)
    {
        try
        {
            var applicationUsers = await _userManager.Users.AsQueryable().OrderBy(x => x.Id)
                .Skip(pageNumber * pageSize).Take(pageSize).ToListAsync();

            var userModelList = new List<UserInfoModel>();
            foreach (var applicationUser in applicationUsers)
                userModelList.Add(new UserInfoModel
                {
                    FirstName = applicationUser.FirstName,
                    LastName = applicationUser.LastName,
                    UserName = applicationUser.UserName,
                    Email = applicationUser.Email,
                    UserId = applicationUser.Id.ToString(),
                    Roles = (await _userManager.GetRolesAsync(applicationUser)).ToList()
                });

            return new ApiResponse(StatusCodes.Status200OK, "User list fetched", userModelList);
        }
        catch (Exception ex)
        {
            throw new Exception(null, ex);
        }
    }

    private async Task<UserInfoModel> BuildUserInfoModelAsync(ClaimsPrincipal userClaimsPrincipal)
    {
        var user = await _userManager.GetUserAsync(userClaimsPrincipal);
        if (user != null)
        {
            try
            {
                return new UserInfoModel
                {
                    IsAuthenticated = userClaimsPrincipal.Identity.IsAuthenticated,
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserId = user.Id.ToString(),
                    ExposedClaims = userClaimsPrincipal.Claims
                        .Select(c => new KeyValuePair<string, string>(c.Type, c.Value)).ToList(),
                    Roles = ((ClaimsIdentity)userClaimsPrincipal.Identity).Claims
                        .Where(c => c.Type == ScopeConstants.Role).Select(c => c.Value).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Could not build UserInfoModel: " + ex.Message);
            }

            return null;
        }

        return new UserInfoModel();
    }
}