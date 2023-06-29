using BlazorApp.Common.Constants;
using FluentValidation;

namespace BlazorApp.Common.Models.Validators;

public class RegisterModelValidator : AbstractValidator<RegisterModel>
{
    public RegisterModelValidator()
    {
        RuleFor(x => x.UserName).NotNull().NotEmpty().MinimumLength(2)
            .MaximumLength(FieldConstants.QuarterFieldLength).Matches(@"[^\s]+")
            .WithMessage("Spaces are not permitted.");
        RuleFor(x => x.Email).NotNull().NotEmpty().EmailAddress();
        RuleFor(x => x.Password).Password();
        RuleFor(x => x.PasswordConfirm).NotNull().NotEmpty().Equal(x => x.Password)
            .WithMessage("The password and confirmation password do not match.");
    }
}