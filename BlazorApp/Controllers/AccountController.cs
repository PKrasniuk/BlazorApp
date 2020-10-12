using BlazorApp.BLL.Interfaces;
using BlazorApp.Common.Models;
using BlazorApp.Common.Models.Security;
using BlazorApp.Common.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BlazorApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountManager _accountManager;

        public AccountController(IAccountManager accountManager)
        {
            _accountManager = accountManager;
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<ApiResponse> LoginAsync([FromBody] LoginModel model)
        {
            return !ModelState.IsValid
                ? new ApiResponse(StatusCodes.Status400BadRequest, "Login Model is Invalid")
                : await _accountManager.LoginAsync(model);
        }

        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<ApiResponse> RegisterAsync([FromBody] RegisterModel model)
        {
            return !ModelState.IsValid
                ? new ApiResponse(StatusCodes.Status400BadRequest, "Register Model is Invalid")
                : await _accountManager.RegisterAsync(model);
        }

        [HttpPost("ConfirmEmail")]
        [AllowAnonymous]
        public async Task<ApiResponse> ConfirmEmailAsync([FromBody] ConfirmEmailModel model)
        {
            return !ModelState.IsValid
                ? new ApiResponse(StatusCodes.Status400BadRequest, "Confirm Email Model is Invalid")
                : await _accountManager.ConfirmEmailAsync(model);
        }

        [HttpPost("ForgotPassword")]
        [AllowAnonymous]
        public async Task<ApiResponse> ForgotPasswordAsync([FromBody] ForgotPasswordModel model)
        {
            return !ModelState.IsValid
                ? new ApiResponse(StatusCodes.Status400BadRequest, "Forgot Password Model is Invalid")
                : await _accountManager.ForgotPasswordAsync(model);
        }

        [HttpPost("ResetPassword")]
        [AllowAnonymous]
        public async Task<ApiResponse> ResetPasswordAsync([FromBody] ResetPasswordModel model)
        {
            return !ModelState.IsValid
                ? new ApiResponse(StatusCodes.Status400BadRequest, "Reset Password Model is Invalid")
                : await _accountManager.ResetPasswordAsync(model);
        }

        [HttpPost("Logout")]
        [Authorize]
        public async Task<ApiResponse> LogoutAsync()
        {
            return await _accountManager.LogoutAsync();
        }

        [HttpGet("UserInfo")]
        public async Task<ApiResponse> UserInfoAsync()
        {
            return await _accountManager.UserInfoAsync(User);
        }

        [HttpPost("UpdateUser")]
        [Authorize]
        public async Task<ApiResponse> UpdateUserAsync([FromBody] UserInfoModel model)
        {
            return !ModelState.IsValid
                ? new ApiResponse(StatusCodes.Status400BadRequest, "User Info Model is Invalid")
                : await _accountManager.UpdateUserAsync(model);
        }

        [HttpPost("Create")]
        [Authorize(Permissions.User.Create)]
        public async Task<ApiResponse> CreateAsync([FromBody] RegisterModel model)
        {
            return !ModelState.IsValid
                ? new ApiResponse(StatusCodes.Status400BadRequest, "Register Model is Invalid")
                : await _accountManager.CreateAsync(model);
        }

        [HttpDelete("{id}")]
        [Authorize(Permissions.User.Delete)]
        public async Task<ApiResponse> DeleteAsync(string id)
        {
            return await _accountManager.DeleteAsync(id);
        }

        [HttpGet("GetUser")]
        public async Task<ApiResponse> GetUserAsync()
        {
            return await _accountManager.GetUserAsync(User);
        }

        [HttpGet("ListRoles")]
        [Authorize(Permissions.Role.Read)]
        public async Task<ApiResponse> ListRolesAsync()
        {
            return await _accountManager.ListRolesAsync();
        }

        [HttpPut]
        [Authorize(Permissions.User.Update)]
        public async Task<ApiResponse> UpdateAsync([FromBody] UserInfoModel model)
        {
            return !ModelState.IsValid
                ? new ApiResponse(StatusCodes.Status400BadRequest, "User Model is Invalid")
                : await _accountManager.UpdateAsync(model);
        }

        [HttpPost("AdminUserPasswordReset/{id}")]
        [Authorize(Policy = Policies.IsAdmin)]
        public async Task<ApiResponse> AdminResetUserPasswordAsync(string id, [FromBody] string newPassword)
        {
            return !ModelState.IsValid
                ? new ApiResponse(StatusCodes.Status400BadRequest, "Model is Invalid")
                : await _accountManager.AdminResetUserPasswordAsync(id, newPassword, User);
        }
    }
}