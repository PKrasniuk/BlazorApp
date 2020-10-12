using BlazorApp.Common.Models;
using BlazorApp.Common.Wrappers;
using System.Threading.Tasks;

namespace BlazorApp.BLL.Interfaces
{
    public interface IMessageManager
    {
        Task<ApiResponse> CreateMessageAsync(MessageModel messageModel);

        Task<ApiResponse> GetMessagesAsync();

        Task<ApiResponse> DeleteMessageAsync(string id);
    }
}