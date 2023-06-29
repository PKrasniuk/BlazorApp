using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BlazorApp.Common.Models;
using BlazorApp.Common.Wrappers;
using BlazorApp.CommonUI.Services.Contracts;
using Microsoft.JSInterop;
using Newtonsoft.Json;
#if !ClientSideBlazor
using System.Linq;
using System.Net;
using System.Net.Http.Json;
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
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "api/Account/Login")
            {
                Content = new StringContent(JsonConvert.SerializeObject(model))
            };
            httpRequestMessage.Content.Headers.ContentType =
                new MediaTypeHeaderValue("application/json");

            using var response = await _httpClient.SendAsync(httpRequestMessage);
            response.EnsureSuccessStatusCode();

#if !ClientSideBlazor
            if (response.Headers.TryGetValues("Set-Cookie", out var cookieEntries))
                if (response.RequestMessage != null)
                {
                    var uri = response.RequestMessage.RequestUri;
                    var cookieContainer = new CookieContainer();
                    foreach (var cookieEntry in cookieEntries)
                        if (uri != null)
                            cookieContainer.SetCookies(uri, cookieEntry);

                    if (uri != null)
                    {
                        var cookies = cookieContainer.GetCookies(uri).Cast<Cookie>();
                        foreach (var cookie in cookies)
                            await _jsRuntime.InvokeVoidAsync("jsInterops.setCookie", cookie.ToString());
                    }
                }
#endif
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ApiResponse>(content);

            return result;
        }

        public async Task<ApiResponse> CreateAsync(RegisterModel model)
        {
            var apiResponse = await _httpClient.PostAsJsonAsync("api/Account/Create", model);
            return await ApiResponse.ReturnApiResponse(apiResponse);
        }

        public async Task<ApiResponse> RegisterAsync(RegisterModel model)
        {
            var apiResponse = await _httpClient.PostAsJsonAsync("api/Account/Register", model);
            return await ApiResponse.ReturnApiResponse(apiResponse);
        }

        public async Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordModel model)
        {
            var apiResponse = await _httpClient.PostAsJsonAsync("api/Account/ForgotPassword", model);
            return await ApiResponse.ReturnApiResponse(apiResponse);
        }

        public async Task<ApiResponse> ResetPasswordAsync(ResetPasswordModel model)
        {
            var apiResponse = await _httpClient.PostAsJsonAsync("api/Account/ResetPassword", model);
            return await ApiResponse.ReturnApiResponse(apiResponse);
        }

        public async Task<ApiResponse> LogoutAsync()
        {
            List<string> cookies = null;

#if !ClientSideBlazor
            if (_httpClient.DefaultRequestHeaders.TryGetValues(CommonConstants.CookieName, out var cookieEntries))
                cookies = cookieEntries.ToList();
#endif
            var apiResponse = await _httpClient.PostAsync("api/Account/Logout", null);

#if !ClientSideBlazor
            if (apiResponse.StatusCode == HttpStatusCode.OK && cookies != null && cookies.Any())
            {
                _httpClient.DefaultRequestHeaders.Remove(CommonConstants.CookieName);
                foreach (var cookie in cookies[0].Split(';'))
                {
                    var cookieParts = cookie.Split('=');
                    await _jsRuntime.InvokeVoidAsync("jsInterops.removeCookie", cookieParts[0]);
                }
            }
#endif
            return await ApiResponse.ReturnApiResponse(apiResponse);
        }

        public async Task<ApiResponse> ConfirmEmailAsync(ConfirmEmailModel model)
        {
            var apiResponse = await _httpClient.PostAsJsonAsync("api/Account/ConfirmEmail", model);
            return await ApiResponse.ReturnApiResponse(apiResponse);
        }

        public async Task<UserInfoModel> GetUserInfoAsync()
        {
            var userInfo = new UserInfoModel { IsAuthenticated = false, Roles = new List<string>() };
            var apiResponse = await _httpClient.GetAsync("api/Account/UserInfo");

            return apiResponse.StatusCode == HttpStatusCode.OK
                ? JsonConvert.DeserializeObject<UserInfoModel>(await apiResponse.Content.ReadAsStringAsync())
                : userInfo;
        }

        public async Task<ApiResponse> UpdateUserAsync(UserInfoModel model)
        {
            var apiResponse = await _httpClient.PostAsJsonAsync("api/Account/UpdateUser", model);
            return await ApiResponse.ReturnApiResponse(apiResponse);
        }

        public async Task<UserInfoModel> GetUserAsync()
        {
            var apiResponse = await _httpClient.GetAsync("api/Account/GetUser");
            return JsonConvert.DeserializeObject<UserInfoModel>(await apiResponse.Content.ReadAsStringAsync());
        }
    }
}