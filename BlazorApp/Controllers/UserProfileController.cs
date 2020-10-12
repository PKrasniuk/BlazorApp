using BlazorApp.BLL.Interfaces;
using BlazorApp.Common.Models;
using BlazorApp.Common.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BlazorApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserProfileController : ControllerBase
    {
        private readonly IUserProfileManager _userProfileManager;

        public UserProfileController(IUserProfileManager userProfileManager)
        {
            _userProfileManager = userProfileManager;
        }

        [HttpGet]
        public async Task<ApiResponse> Get()
        {
            return await _userProfileManager.GetUserProfileAsync();
        }

        [HttpGet("{id}")]
        public async Task<ApiResponse> Get(string id)
        {
            return await _userProfileManager.GetUserProfileAsync(id);
        }

        [HttpPost("Upsert")]
        public async Task<ApiResponse> UpsertUserProfileAsync(UserProfileModel model)
        {
            return ModelState.IsValid
                ? await _userProfileManager.UpsertUserProfileAsync(model)
                : new ApiResponse(StatusCodes.Status400BadRequest, "User Model is Invalid");
        }
    }
}