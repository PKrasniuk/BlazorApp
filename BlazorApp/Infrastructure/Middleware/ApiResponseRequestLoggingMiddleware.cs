using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BlazorApp.BLL.Infrastructure.Exceptions;
using BlazorApp.BLL.Infrastructure.Extensions;
using BlazorApp.BLL.Interfaces;
using BlazorApp.Common.Helpers;
using BlazorApp.Common.Models;
using BlazorApp.Common.Models.Enums;
using BlazorApp.Common.Wrappers;
using BlazorApp.Domain.Entities;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace BlazorApp.Infrastructure.Middleware;

public class ApiResponseRequestLoggingMiddleware
{
    private readonly bool _enableApiLogging;
    private readonly List<string> _ignorePaths;
    private readonly RequestDelegate _next;
    private IApiLogManager _apiLogManager;
    private ILogger<ApiResponseRequestLoggingMiddleware> _logger;

    public ApiResponseRequestLoggingMiddleware(RequestDelegate next, bool enableApiLogging,
        IConfiguration configuration)
    {
        _next = next;
        _enableApiLogging = enableApiLogging;
        _ignorePaths = configuration.GetSection("BlazorApp:APILogging:IgnorePaths").Get<List<string>>();
    }

    public async Task Invoke(HttpContext httpContext, IApiLogManager apiLogManager,
        ILogger<ApiResponseRequestLoggingMiddleware> logger, UserManager<ApplicationUser<Guid>> userManager)
    {
        _logger = logger;
        _apiLogManager = apiLogManager;

        try
        {
            var request = httpContext.Request;
            if (IsSwagger(httpContext) || !request.Path.StartsWithSegments(new PathString("/api")))
            {
                await _next(httpContext);
            }
            else
            {
                var stopWatch = Stopwatch.StartNew();
                var requestTime = DateTime.UtcNow;

                var formattedRequest = await FormatRequest(request);
                var originalBodyStream = httpContext.Response.Body;

                await using var responseBody = new MemoryStream();
                httpContext.Response.Body = responseBody;

                try
                {
                    var response = httpContext.Response;
                    response.Body = responseBody;
                    await _next.Invoke(httpContext);

                    string responseBodyContent = null;

                    if (httpContext.Response.StatusCode == StatusCodes.Status200OK)
                    {
                        responseBodyContent = await FormatResponse(response);
                        await HandleSuccessRequestAsync(httpContext, responseBodyContent, StatusCodes.Status200OK);
                    }
                    else
                    {
                        await HandleNotSuccessRequestAsync(httpContext, httpContext.Response.StatusCode);
                    }

                    stopWatch.Stop();

                    if (_enableApiLogging && _ignorePaths.Any(e =>
                            !request.Path.StartsWithSegments(new PathString(e.ToLower()))))
                        try
                        {
                            await responseBody.CopyToAsync(originalBodyStream);

                            var user = httpContext.User.Identity.IsAuthenticated
                                ? await userManager.FindByIdAsync(httpContext.User.Claims
                                    .First(c => c.Type == JwtClaimTypes.Subject).Value)
                                : null;

                            await SafeLog(requestTime,
                                stopWatch.ElapsedMilliseconds,
                                response.StatusCode,
                                request.Method,
                                request.Path,
                                request.QueryString.ToString(),
                                formattedRequest,
                                responseBodyContent,
                                httpContext.Connection.RemoteIpAddress.ToString(),
                                user
                            );
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning("An Inner Middleware exception occurred on SafeLog: " + ex.Message);
                        }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("An Inner Middleware exception occurred: " + ex.Message);
                    await HandleExceptionAsync(httpContext, ex);
                }
                finally
                {
                    responseBody.Seek(0, SeekOrigin.Begin);
                    await responseBody.CopyToAsync(originalBodyStream);
                }
            }
        }
        catch (Exception ex)
        {
            if (httpContext.Response.HasStarted)
            {
                _logger.LogWarning("A Middleware exception occurred, but response has already started!");
                throw;
            }

            await HandleExceptionAsync(httpContext, ex);
            throw;
        }
    }

    private async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
    {
        _logger.LogError("Api Exception:", exception);

        ApiError apiError;
        int code;

        switch (exception)
        {
            case ApiException ex:
                apiError = new ApiError(ResponseMessageEnum.ValidationError.GetDescription(), ex.Errors)
                {
                    ValidationErrors = ex.Errors,
                    ReferenceErrorCode = ex.ReferenceErrorCode,
                    ReferenceDocumentLink = ex.ReferenceDocumentLink
                };
                code = ex.StatusCode;
                httpContext.Response.StatusCode = code;
                break;
            case UnauthorizedAccessException _:
                apiError = new ApiError("Unauthorized Access");
                code = StatusCodes.Status401Unauthorized;
                httpContext.Response.StatusCode = code;
                break;
            default:
                apiError = new ApiError(exception.GetBaseException().Message)
                {
                    Details = exception.StackTrace
                };
                code = StatusCodes.Status500InternalServerError;
                httpContext.Response.StatusCode = code;
                break;
        }

        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(new ApiResponse(code,
            ResponseMessageEnum.Exception.GetDescription(), null, apiError)));
    }

    private static Task HandleNotSuccessRequestAsync(HttpContext httpContext, int code)
    {
        ApiError apiError;

        switch (code)
        {
            case StatusCodes.Status404NotFound:
                apiError = new ApiError(ResponseMessageEnum.NotFound.GetDescription());
                break;
            case StatusCodes.Status204NoContent:
                apiError = new ApiError(ResponseMessageEnum.NotContent.GetDescription());
                break;
            case StatusCodes.Status405MethodNotAllowed:
                apiError = new ApiError(ResponseMessageEnum.MethodNotAllowed.GetDescription());
                break;
            case StatusCodes.Status401Unauthorized:
                apiError = new ApiError(ResponseMessageEnum.UnAuthorized.GetDescription());
                break;
            default:
                apiError = new ApiError(ResponseMessageEnum.Unknown.GetDescription());
                break;
        }

        var apiResponse = new ApiResponse(code, apiError);
        httpContext.Response.StatusCode = code;
        httpContext.Response.ContentType = "application/json";
        return httpContext.Response.WriteAsync(JsonConvert.SerializeObject(apiResponse));
    }

    private static Task HandleSuccessRequestAsync(HttpContext httpContext, object body, int code)
    {
        string jsonString;

        ApiResponse apiResponse;

        if (!body.ToString().IsValidJson())
            return httpContext.Response.WriteAsync(JsonConvert.SerializeObject(null));

        var bodyText = body.ToString();

        var bodyContent = JsonConvert.DeserializeObject<dynamic>(bodyText);

        Type type = bodyContent?.GetType();

        if (type == typeof(JObject))
        {
            apiResponse = JsonConvert.DeserializeObject<ApiResponse>(bodyText);

            if (apiResponse.StatusCode == 0) apiResponse.StatusCode = code;

            if (apiResponse.Result != null || !string.IsNullOrEmpty(apiResponse.Message))
            {
                jsonString = JsonConvert.SerializeObject(apiResponse);
            }
            else
            {
                apiResponse = new ApiResponse(code, ResponseMessageEnum.Success.GetDescription(), bodyContent,
                    null);
                jsonString = JsonConvert.SerializeObject(apiResponse);
            }
        }
        else
        {
            apiResponse = new ApiResponse(code, ResponseMessageEnum.Success.GetDescription(), bodyContent, null);
            jsonString = JsonConvert.SerializeObject(apiResponse);
        }

        httpContext.Response.ContentType = "application/json";
        return httpContext.Response.WriteAsync(jsonString);
    }

    private static async Task<string> FormatRequest(HttpRequest request)
    {
        request.EnableBuffering();

        var buffer = new byte[Convert.ToInt32(request.ContentLength)];
        await request.Body.ReadAsync(buffer, 0, buffer.Length);
        var bodyAsText = Encoding.UTF8.GetString(buffer);
        request.Body.Seek(0, SeekOrigin.Begin);

        return $"{request.Method} {request.Scheme} {request.Host}{request.Path} {request.QueryString} {bodyAsText}";
    }

    private static async Task<string> FormatResponse(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        var plainBodyText = await new StreamReader(response.Body).ReadToEndAsync();
        response.Body.Seek(0, SeekOrigin.Begin);
        return plainBodyText;
    }

    private async Task SafeLog(DateTime requestTime, long responseMillis, int statusCode, string method,
        string path, string queryString, string requestBody, string responseBody, string ipAddress,
        IdentityUser<Guid> user)
    {
        if (path.ToLower().StartsWith("/api/account/") || path.ToLower().StartsWith("/api/UserProfile/")) return;

        if (requestBody.Length > 256) requestBody = $"(Truncated to 200 chars) {requestBody.Substring(0, 200)}";

        if (responseBody != null && responseBody.Contains("\"result\":"))
        {
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(responseBody);
            responseBody = Regex.Replace(apiResponse.Result.ToString(), @"(""[^""\\]*(?:\\.[^""\\]*)*"")|\s+",
                "$1");
        }

        if (responseBody != null && responseBody.Length > 256)
            responseBody = $"(Truncated to 200 chars) {responseBody.Substring(0, 200)}";

        if (queryString.Length > 256) queryString = $"(Truncated to 200 chars) {queryString.Substring(0, 200)}";

        await _apiLogManager.LogAsync(new ApiLogItemModel
        {
            RequestTime = requestTime,
            ResponseMillis = responseMillis,
            StatusCode = statusCode,
            Method = method,
            Path = path,
            QueryString = queryString,
            RequestBody = requestBody,
            ResponseBody = responseBody ?? string.Empty,
            IpAddress = ipAddress,
            ApplicationUserId = user?.Id.ToString()
        });
    }

    private static string ConvertToJsonString(object rawJson)
    {
        return JsonConvert.SerializeObject(rawJson, JsonSettings());
    }

    private static JsonSerializerSettings JsonSettings()
    {
        return new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new List<JsonConverter> { new StringEnumConverter() }
        };
    }

    private static bool IsSwagger(HttpContext context)
    {
        return context.Request.Path.StartsWithSegments("/swagger");
    }
}