using System;
using System.Linq;
using System.Threading.Tasks;
using BlazorApp.BLL.Infrastructure.Exceptions;
using BlazorApp.Common.Helpers;
using BlazorApp.Common.Interfaces;
using BlazorApp.Common.Models.Enums;
using BlazorApp.Common.Wrappers;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BlazorApp.Infrastructure.Middleware;

public class UserSessionMiddleware
{
    private readonly RequestDelegate _next;
    private ILogger<UserSessionMiddleware> _logger;

    public UserSessionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, ILogger<UserSessionMiddleware> logger,
        IUserSession<Guid> userSession)
    {
        _logger = logger;

        try
        {
            if (httpContext.User.Identity.IsAuthenticated)
            {
                userSession.UserId =
                    new Guid(httpContext.User.Claims.First(c => c.Type == JwtClaimTypes.Subject).Value);
                userSession.UserName = httpContext.User.Identity.Name;
                userSession.Roles = httpContext.User.Claims.Where(c => c.Type == JwtClaimTypes.Role)
                    .Select(c => c.Value).ToList();
            }

            await _next.Invoke(httpContext);
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
}