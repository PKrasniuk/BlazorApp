using BlazorApp.BLL.Interfaces;
using BlazorApp.Common.Models.Security;
using BlazorApp.Common.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BlazorApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Policies.IsAdmin)]
    public class DbLogController : ControllerBase
    {
        private readonly IDbLogManager _dbLogManager;

        public DbLogController(IDbLogManager dbLogManager)
        {
            _dbLogManager = dbLogManager;
        }

        [HttpGet]
        public async Task<ApiResponse> GetLogsAsync([FromQuery] int pageSize, [FromQuery] int page)
        {
            return await _dbLogManager.GetAsync(pageSize, page);
        }
    }
}