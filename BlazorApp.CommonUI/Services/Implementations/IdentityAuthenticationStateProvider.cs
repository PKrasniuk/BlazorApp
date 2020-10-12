using BlazorApp.Common.Constants;
using BlazorApp.Common.Models;
using BlazorApp.Common.Wrappers;
using BlazorApp.CommonUI.Services.Contracts;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazorApp.CommonUI.Services.Implementations
{
    public class IdentityAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly AppState _appState;
        private readonly IAuthorizeApi _authorizeApi;

        public IdentityAuthenticationStateProvider(IAuthorizeApi authorizeApi, AppState appState)
        {
            _authorizeApi = authorizeApi;
            _appState = appState;
        }

        public async Task<ApiResponse> Login(LoginModel model)
        {
            var apiResponse = await _authorizeApi.LoginAsync(model);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }

        public async Task<ApiResponse> Register(RegisterModel model)
        {
            var apiResponse = await _authorizeApi.RegisterAsync(model);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }

        public async Task<ApiResponse> Create(RegisterModel model)
        {
            return await _authorizeApi.CreateAsync(model);
        }

        public async Task<ApiResponse> Logout()
        {
            _appState.UserProfile = null;
            var apiResponse = await _authorizeApi.LogoutAsync();
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }

        public async Task<ApiResponse> ConfirmEmail(ConfirmEmailModel model)
        {
            var apiResponse = await _authorizeApi.ConfirmEmailAsync(model);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }

        public async Task<ApiResponse> ResetPassword(ResetPasswordModel model)
        {
            var apiResponse = await _authorizeApi.ResetPasswordAsync(model);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }

        public async Task<ApiResponse> ForgotPassword(ForgotPasswordModel model)
        {
            return await _authorizeApi.ForgotPasswordAsync(model);
        }

        public async Task<UserInfoModel> GetUserInfo()
        {
            var userInfo = await _authorizeApi.GetUserAsync();
            return userInfo.IsAuthenticated
                ? await _authorizeApi.GetUserInfoAsync()
                : new UserInfoModel { IsAuthenticated = false, Roles = new List<string>() };
        }

        public async Task<ApiResponse> UpdateUser(UserInfoModel model)
        {
            var apiResponse = await _authorizeApi.UpdateUserAsync(model);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return apiResponse;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = new ClaimsIdentity();
            try
            {
                var userInfo = await GetUserInfo();
                if (userInfo.IsAuthenticated)
                {
                    var claims = new[] { new Claim(ClaimTypes.Name, userInfo.UserName) };
                    if (userInfo.ExposedClaims != null && userInfo.ExposedClaims.Any())
                        claims = claims.Concat(userInfo.ExposedClaims.Select(c => new Claim(c.Key, c.Value))).ToArray();
                    identity = new ClaimsIdentity(claims, "Server authentication", "name", ScopeConstants.Role);
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("Request failed:" + ex);
            }

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
    }
}