using BlazorApp.Common.Models.EmailModels;
using BlazorApp.Common.Wrappers;
using System.Threading.Tasks;

namespace BlazorApp.BLL.Interfaces
{
    public interface IEmailManager
    {
        Task<ApiResponse> SendEmailAsync(EmailMessageModel emailMessage);

        Task<ApiResponse> SendEmailWrapperAsync(EmailModel model);

        Task<ApiResponse> ReceiveMailImapAsync();
    }
}