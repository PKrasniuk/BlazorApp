using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BlazorApp.Common.Models;
using BlazorApp.Common.Wrappers;
using BlazorApp.CommonUI.Services.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.JSInterop;
using Newtonsoft.Json;
#if !ClientSideBlazor
using System.Linq;
using System.Net;
#endif
#if !ClientSideBlazor
using BlazorApp.Common.Constants;
#endif

namespace BlazorApp.CommonUI.Services.Implementations
{
    public class AuthorizeApi : IAuthorizeApi
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        public AuthorizeApi(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public async Task<ApiResponse> LoginAsync(LoginModel model)
        {
            ApiResponse result;

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "api/Account/Login")
            {
                Content = new StringContent(JsonConvert.SerializeObject(model))
            };
            httpRequestMessage.Content.Headers.ContentType =
                new MediaTypeHeaderValue("application/json");

            using (var response = await _httpClient.SendAsync(httpRequestMessage))
            {
                response.EnsureSuccessStatusCode();

#if !ClientSideBlazor
                if (response.Headers.TryGetValues("Set-Cookie", out var cookieEntries))
                {
                    var uri = response.RequestMessage.RequestUri;
                    var cookieContainer = new CookieContainer();
                    foreach (var cookieEntry in cookieEntries) cookieContainer.SetCookies(uri, cookieEntry);

                    var cookies = cookieContainer.GetCookies(uri).Cast<Cookie>();
                    foreach (var cookie in cookies)
                        await _jsRuntime.InvokeVoidAsync("jsInterops.setCookie", cookie.ToString());
                }
#endif
                var content = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<ApiResponse>(content);
            }

            return result;
        }

        public async Task<ApiResponse> CreateAsync(RegisterModel model)
        {
            return await _httpClient.PostJsonAsync<ApiResponse>("api/Account/Create", model);
        }

        public async Task<ApiResponse> RegisterAsync(RegisterModel model)
        {
            return await _httpClient.PostJsonAsync<ApiResponse>("api/Account/Register", model);
        }

        public async Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordModel model)
        {
            return await _httpClient.PostJsonAsync<ApiResponse>("api/Account/ForgotPassword", model);
        }

        public async Task<ApiResponse> ResetPasswordAsync(ResetPasswordModel model)
        {
            return await _httpClient.PostJsonAsync<ApiResponse>("api/Account/ResetPassword", model);
        }

        public async Task<ApiResponse> LogoutAsync()
        {
            List<string> cookies = null;

#if !ClientSideBlazor
            if (_httpClient.DefaultRequestHeaders.TryGetValues(CommonConstants.CookieName, out var cookieEntries))
                cookies = cookieEntries.ToList();
#endif
            var response = await _httpClient.PostJsonAsync<ApiResponse>("api/Account/Logout", null);

#if !ClientSideBlazor
            if (response.StatusCode == StatusCodes.Status200OK && cookies != null && cookies.Any())
            {
                _httpClient.DefaultRequestHeaders.Remove(CommonConstants.CookieName);
                foreach (var cookie in cookies[0].Split(';'))
                {
                    var cookieParts = cookie.Split('=');
                    await _jsRuntime.InvokeVoidAsync("jsInterops.removeCookie", cookieParts[0]);
                }
            }
#endif
            return response;
        }

        public async Task<ApiResponse> ConfirmEmailAsync(ConfirmEmailModel model)
        {
            return await _httpClient.PostJsonAsync<ApiResponse>("api/Account/ConfirmEmail", model);
        }

        public async Task<UserInfoModel> GetUserInfoAsync()
        {
            var userInfo = new UserInfoModel { IsAuthenticated = false, Roles = new List<string>() };
            var apiResponse = await _httpClient.GetJsonAsync<ApiResponse>("api/Account/UserInfo");

            return apiResponse.StatusCode == StatusCodes.Status200OK
                ? JsonConvert.DeserializeObject<UserInfoModel>(apiResponse.Result.ToString())
                : userInfo;
        }

        public async Task<ApiResponse> UpdateUserAsync(UserInfoModel model)
        {
            return await _httpClient.PostJsonAsync<ApiResponse>("api/Account/UpdateUser", model);
        }

        public async Task<UserInfoModel> GetUserAsync()
        {
            var apiResponse = await _httpClient.GetJsonAsync<ApiResponse>("api/Account/GetUser");
            return JsonConvert.DeserializeObject<UserInfoModel>(apiResponse.Result.ToString());
        }
    }
}