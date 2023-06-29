using System.Threading.Tasks;
using BlazorApp.BLL.Interfaces;
using BlazorApp.Common.Models.Security;
using BlazorApp.Common.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazorApp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ApiLogController : ControllerBase
{
    private readonly IApiLogManager _apiLogManager;

    public ApiLogController(IApiLogManager apiLogManager)
    {
        _apiLogManager = apiLogManager;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ApiResponse> GetApiResponsesAsync()
    {
        return await _apiLogManager.GetApiResponsesAsync();
    }

    [HttpGet("{userId}")]
    [Authorize(Policy = Policies.IsAdmin)]
    public async Task<ApiResponse> GetApiResponseByApplicationUserId(string userId)
    {
        return await _apiLogManager.GetApiResponseByApplicationUserIdAsync(userId);
    }
}