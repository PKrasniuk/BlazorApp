using System.Threading.Tasks;
using BlazorApp.Common.Models.EmailModels;
using BlazorApp.Common.Wrappers;

namespace BlazorApp.BLL.Interfaces;

public interface IEmailManager
{
    Task<ApiResponse> SendEmailAsync(EmailMessageModel emailMessage);

    Task<ApiResponse> SendEmailWrapperAsync(EmailModel model);

    Task<ApiResponse> ReceiveMailImapAsync();
}