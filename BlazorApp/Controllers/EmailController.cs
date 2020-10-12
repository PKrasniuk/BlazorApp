using BlazorApp.BLL.Interfaces;
using BlazorApp.Common.Models.EmailModels;
using BlazorApp.Common.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BlazorApp.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IEmailManager _emailManager;

        public EmailController(IEmailManager emailManager)
        {
            _emailManager = emailManager;
        }

        [HttpPost("Send")]
        public async Task<ApiResponse> SendEmailAsync([FromBody] EmailModel model)
        {
            return !ModelState.IsValid
                ? new ApiResponse(StatusCodes.Status400BadRequest, "User Model is Invalid")
                : await _emailManager.SendEmailWrapperAsync(model);
        }

        [HttpGet("Receive")]
        [Authorize]
        public async Task<ApiResponse> ReceiveEmailAsync()
        {
            return await _emailManager.ReceiveMailImapAsync();
        }
    }
}