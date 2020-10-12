using BlazorApp.Common.Models;
using BlazorApp.Common.Wrappers;
using System.Threading.Tasks;

namespace BlazorApp.CommonUI.Services.Contracts
{
    public interface IAuthorizeApi
    {
        Task<ApiResponse> LoginAsync(LoginModel model);

        Task<ApiResponse> CreateAsync(RegisterModel model);

        Task<ApiResponse> RegisterAsync(RegisterModel model);

        Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordModel model);

        Task<ApiResponse> ResetPasswordAsync(ResetPasswordModel model);

        Task<ApiResponse> LogoutAsync();

        Task<ApiResponse> ConfirmEmailAsync(ConfirmEmailModel model);

        Task<UserInfoModel> GetUserInfoAsync();

        Task<ApiResponse> UpdateUserAsync(UserInfoModel model);

        Task<UserInfoModel> GetUserAsync();
    }
}