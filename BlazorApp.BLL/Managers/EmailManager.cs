using BlazorApp.BLL.Infrastructure.Helpers;
using BlazorApp.BLL.Interfaces;
using BlazorApp.Common.Constants;
using BlazorApp.Common.Interfaces;
using BlazorApp.Common.Models.EmailModels;
using BlazorApp.Common.Wrappers;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp.BLL.Managers
{
    public class EmailManager : IEmailManager
    {
        private readonly IEmailConfiguration _emailConfiguration;
        private readonly ILogger<EmailManager> _logger;

        public EmailManager(IEmailConfiguration emailConfiguration, ILogger<EmailManager> logger)
        {
            _emailConfiguration = emailConfiguration;
            _logger = logger;
        }

        public async Task<ApiResponse> SendEmailAsync(EmailMessageModel emailMessage)
        {
            try
            {
                var message = new MimeMessage();
                if (!emailMessage.FromAddresses.Any())
                    emailMessage.FromAddresses.Add(new EmailAddressModel(_emailConfiguration.FromName,
                        _emailConfiguration.FromAddress));

                message.To.AddRange(emailMessage.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
                message.From.AddRange(emailMessage.FromAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
                message.Cc.AddRange(emailMessage.CcAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
                message.Bcc.AddRange(emailMessage.BccAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
                message.Subject = emailMessage.Subject;
                message.Body = emailMessage.IsHtml
                    ? new BodyBuilder { HtmlBody = emailMessage.Body }.ToMessageBody()
                    : new TextPart("plain") { Text = emailMessage.Body };

                using var emailClient = new SmtpClient();
                if (!_emailConfiguration.SmtpUseSSL)
                    emailClient.ServerCertificateValidationCallback =
                        (sender2, certificate, chain, sslPolicyErrors) => true;

                await emailClient.ConnectAsync(_emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort,
                    _emailConfiguration.SmtpUseSSL).ConfigureAwait(false);
                emailClient.AuthenticationMechanisms.Remove(CommonConstants.AuthenticationMechanismsConst);

                if (!string.IsNullOrWhiteSpace(_emailConfiguration.SmtpUsername))
                    await emailClient
                        .AuthenticateAsync(_emailConfiguration.SmtpUsername, _emailConfiguration.SmtpPassword)
                        .ConfigureAwait(false);

                await emailClient.SendAsync(message).ConfigureAwait(false);
                await emailClient.DisconnectAsync(true).ConfigureAwait(false);
                return new ApiResponse(StatusCodes.Status203NonAuthoritative);
            }
            catch (Exception ex)
            {
                _logger.LogError("Email Send Failed: {0}", ex.Message);
                return new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse> SendEmailWrapperAsync(EmailModel model)
        {
            var email = new EmailMessageModel();
            email.ToAddresses.Add(new EmailAddressModel(model.ToName, model.ToAddress));

            if (!string.IsNullOrEmpty(model.TemplateName))
            {
                switch (model.TemplateName)
                {
                    case "PlainTextTestEmail":
                        email.GetPlainTextTestEmail(DateTime.Now);
                        break;
                    default:
                        email.GetPlainTextTestEmail(DateTime.Now);
                        break;
                }
            }
            else
            {
                email.Subject = model.Subject;
                email.Body = model.Body;
            }

            _logger.LogInformation("Test Email: {0}", email.Subject);
            try
            {
                await SendEmailAsync(email);
                return new ApiResponse(StatusCodes.Status200OK, "Email Successfully Sent");
            }
            catch (Exception ex)
            {
                return new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse> ReceiveMailImapAsync()
        {
            using var emailClient = new ImapClient();
            try
            {
                await emailClient.ConnectAsync(_emailConfiguration.ImapServer, _emailConfiguration.ImapPort)
                    .ConfigureAwait(false);
                emailClient.AuthenticationMechanisms.Remove(CommonConstants.AuthenticationMechanismsConst);

                if (!string.IsNullOrWhiteSpace(_emailConfiguration.ImapUsername))
                    await emailClient
                        .AuthenticateAsync(_emailConfiguration.ImapUsername, _emailConfiguration.ImapPassword)
                        .ConfigureAwait(false);

                var emails = new List<EmailMessageModel>();
                await emailClient.Inbox.OpenAsync(FolderAccess.ReadOnly);

                var uIds = await emailClient.Inbox.SearchAsync(SearchQuery.All);
                foreach (var uid in uIds)
                {
                    var message = await emailClient.Inbox.GetMessageAsync(uid);
                    var emailMessage = new EmailMessageModel
                    {
                        Body = !string.IsNullOrEmpty(message.HtmlBody) ? message.HtmlBody : message.TextBody,
                        Subject = message.Subject
                    };
                    emailMessage.ToAddresses.AddRange(message.To.Select(x => (MailboxAddress)x)
                        .Select(x => new EmailAddressModel(x.Name, x.Address)));
                    emailMessage.FromAddresses.AddRange(message.From.Select(x => (MailboxAddress)x)
                        .Select(x => new EmailAddressModel(x.Name, x.Address)));
                    emails.Add(emailMessage);
                }

                await emailClient.DisconnectAsync(true);
                return new ApiResponse(StatusCodes.Status200OK, null, emails);
            }
            catch (Exception ex)
            {
                _logger.LogError("Imap Email Retrieval failed: {0}", ex.Message);
                return new ApiResponse(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}