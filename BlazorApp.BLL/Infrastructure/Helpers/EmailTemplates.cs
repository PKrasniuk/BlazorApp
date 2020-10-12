using BlazorApp.Common.Models.EmailModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.IO;

namespace BlazorApp.BLL.Infrastructure.Helpers
{
    public static class EmailTemplates
    {
        private static IWebHostEnvironment _webHostEnvironment;
        private static string _logoUrl = string.Empty;
        private static string _appUrl = string.Empty;

        private static string _plainTextTestEmailTemplate;
        private static string _newUserConfirmationEmailTemplate;
        private static string _newUserEmailTemplate;
        private static string _newUserNotificationEmailTemplate;
        private static string _passwordResetTemplate;
        private static string _forgotPasswordTemplate;

        public static void Initialize(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            _logoUrl = configuration["BlazorApp:LogoUrl"];
            _appUrl = configuration["BlazorApp:ApplicationUrl"];
        }

        public static EmailMessageModel GetPlainTextTestEmail(this EmailMessageModel emailMessage, DateTime date)
        {
            if (_plainTextTestEmailTemplate == null)
                _plainTextTestEmailTemplate = ReadPhysicalFile("Templates/PlainTextTestEmail.template");

            emailMessage.Body = _plainTextTestEmailTemplate
                .Replace("{date}", date.ToString(CultureInfo.InvariantCulture));
            emailMessage.IsHtml = false;

            return emailMessage;
        }

        public static EmailMessageModel BuildNewUserConfirmationEmail(this EmailMessageModel emailMessage,
            string recipientName, string userName, string callbackUrl, string userId, string token)
        {
            if (_newUserConfirmationEmailTemplate == null)
                _newUserConfirmationEmailTemplate = ReadPhysicalFile("Templates/NewUserConfirmationEmail.template");

            emailMessage.Body = _newUserConfirmationEmailTemplate
                .Replace("{logoUrl}", _logoUrl)
                .Replace("{name}", recipientName)
                .Replace("{userName}", userName)
                .Replace("{callbackUrl}", callbackUrl)
                .Replace("{userId}", userId)
                .Replace("{token}", token);
            emailMessage.Subject = $"Welcome {recipientName} to Blazor App";

            return emailMessage;
        }

        public static EmailMessageModel BuildNewUserEmail(this EmailMessageModel emailMessage, string fullName,
            string userName, string emailAddress, string password)
        {
            if (_newUserEmailTemplate == null)
                _newUserEmailTemplate = ReadPhysicalFile("Templates/NewUserEmail.template");

            emailMessage.Body = _newUserEmailTemplate
                .Replace("{logoUrl}", _logoUrl)
                .Replace("{appUrl}", _appUrl)
                .Replace("{fullName}", fullName)
                .Replace("{fullName}", userName)
                .Replace("{userName}", userName)
                .Replace("{email}", emailAddress)
                .Replace("{password}", password);
            emailMessage.Subject = $"Welcome {fullName} to Blazor App";

            return emailMessage;
        }

        public static EmailMessageModel BuildNewUserNotificationEmail(this EmailMessageModel emailMessage,
            string creator, string name, string userName, string company, string roles)
        {
            if (_newUserNotificationEmailTemplate == null)
                _newUserNotificationEmailTemplate = ReadPhysicalFile("Templates/NewUserEmail.template");

            emailMessage.Body = _newUserNotificationEmailTemplate
                .Replace("{logoUrl}", _logoUrl)
                .Replace("{appUrl}", _appUrl)
                .Replace("{creator}", creator)
                .Replace("{name}", name)
                .Replace("{userName}", userName)
                .Replace("{roles}", roles)
                .Replace("{company}", company);
            emailMessage.Subject = $"A new user [{userName}] has registered on Blazor App";

            return emailMessage;
        }

        public static EmailMessageModel BuildPasswordResetEmail(this EmailMessageModel emailMessage, string userName)
        {
            if (_passwordResetTemplate == null)
                _passwordResetTemplate = ReadPhysicalFile("Templates/PasswordReset.template");

            emailMessage.Body = _passwordResetTemplate
                .Replace("{logoUrl}", _logoUrl)
                .Replace("{userName}", userName);
            emailMessage.Subject = $"Blazor App Password Reset for {userName}";

            return emailMessage;
        }

        public static EmailMessageModel BuildForgotPasswordEmail(this EmailMessageModel emailMessage, string name,
            string callbackUrl, string token)
        {
            if (_forgotPasswordTemplate == null)
                _forgotPasswordTemplate = ReadPhysicalFile("Templates/ForgotPassword.template");

            emailMessage.Body = _forgotPasswordTemplate
                .Replace("{logoUrl}", _logoUrl)
                .Replace("{name}", name)
                .Replace("{token}", token)
                .Replace("{callbackUrl}", callbackUrl);
            emailMessage.Subject = $"Blazor App Forgot your Password? [{name}]";

            return emailMessage;
        }

        private static string ReadPhysicalFile(string path)
        {
            if (_webHostEnvironment == null)
                throw new InvalidOperationException($"{nameof(EmailTemplates)} is not initialized");

            var fileInfo = _webHostEnvironment.ContentRootFileProvider.GetFileInfo(path);
            if (!fileInfo.Exists) throw new FileNotFoundException($"Template file located at \"{path}\" was not found");

            using var fs = fileInfo.CreateReadStream();
            using var sr = new StreamReader(fs);
            return sr.ReadToEnd();
        }
    }
}