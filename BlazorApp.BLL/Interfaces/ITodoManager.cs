using BlazorApp.Common.Models;
using BlazorApp.Common.Wrappers;
using System.Threading.Tasks;

namespace BlazorApp.BLL.Interfaces
{
    public interface ITodoManager
    {
        Task<ApiResponse> GetTodoListAsync();

        Task<ApiResponse> GetTodoAsync(string id);

        Task<ApiResponse> CreateTodoAsync(TodoModel todo);

        Task<ApiResponse> UpdateTodoAsync(TodoModel todo);

        Task<ApiResponse> DeleteTodoAsync(string id);
    }
}