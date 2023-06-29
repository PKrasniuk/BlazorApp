namespace BlazorApp.Common.Models;

public class ResetPasswordModel
{
    public string UserId { get; set; }

    public string Token { get; set; }

    public string Password { get; set; }

    public string PasswordConfirm { get; set; }
}