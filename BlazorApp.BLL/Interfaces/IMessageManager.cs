using System.Threading.Tasks;
using BlazorApp.Common.Models;
using BlazorApp.Common.Wrappers;

namespace BlazorApp.BLL.Interfaces;

public interface IMessageManager
{
    Task<ApiResponse> CreateMessageAsync(MessageModel messageModel);

    Task<ApiResponse> GetMessagesAsync();

    Task<ApiResponse> DeleteMessageAsync(string id);
}