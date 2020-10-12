using BlazorApp.BLL.Interfaces;
using BlazorApp.Common.Models;
using BlazorApp.Common.Models.Security;
using BlazorApp.Common.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BlazorApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly ITodoManager _todoManager;

        public TodoController(ITodoManager todoManager)
        {
            _todoManager = todoManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ApiResponse> Get()
        {
            return await _todoManager.GetTodoListAsync();
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ApiResponse> Get(string id)
        {
            return await _todoManager.GetTodoAsync(id);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ApiResponse> Post([FromBody] TodoModel todoModel)
        {
            return ModelState.IsValid
                ? await _todoManager.CreateTodoAsync(todoModel)
                : new ApiResponse(StatusCodes.Status400BadRequest, "Todo Model is Invalid");
        }

        [HttpPut]
        [AllowAnonymous]
        public async Task<ApiResponse> Put([FromBody] TodoModel todoModel)
        {
            return ModelState.IsValid
                ? await _todoManager.UpdateTodoAsync(todoModel)
                : new ApiResponse(StatusCodes.Status400BadRequest, "Todo Model is Invalid");
        }

        [HttpDelete("{id}")]
        [Authorize(Permissions.Todo.Delete)]
        public async Task<ApiResponse> Delete(string id)
        {
            return await _todoManager.DeleteTodoAsync(id);
        }
    }
}