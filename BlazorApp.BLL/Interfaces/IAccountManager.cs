using System.Security.Claims;
using System.Threading.Tasks;
using BlazorApp.Common.Models;
using BlazorApp.Common.Wrappers;

namespace BlazorApp.BLL.Interfaces;

public interface IAccountManager
{
    Task<ApiResponse> LoginAsync(LoginModel model);

    Task<ApiResponse> RegisterAsync(RegisterModel model);

    Task<ApiResponse> ConfirmEmailAsync(ConfirmEmailModel model);

    Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordModel model);

    Task<ApiResponse> ResetPasswordAsync(ResetPasswordModel model);

    Task<ApiResponse> LogoutAsync();

    Task<ApiResponse> UserInfoAsync(ClaimsPrincipal userClaimsPrincipal);

    Task<ApiResponse> UpdateUserAsync(UserInfoModel model);

    Task<ApiResponse> CreateAsync(RegisterModel model);

    Task<ApiResponse> DeleteAsync(string id);

    Task<ApiResponse> GetUserAsync(ClaimsPrincipal userClaimsPrincipal);

    Task<ApiResponse> ListRolesAsync();

    Task<ApiResponse> UpdateAsync(UserInfoModel model);

    Task<ApiResponse> AdminResetUserPasswordAsync(string id, string newPassword,
        ClaimsPrincipal userClaimsPrincipal);

    Task<ApiResponse> RegisterNewUserAsync(string userName, string email, string password,
        bool requireConfirmEmail);

    Task<ApiResponse> GetUsersAsync(int pageSize = 10, int pageNumber = 0);
}