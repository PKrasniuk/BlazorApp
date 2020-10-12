using FluentValidation;

namespace BlazorApp.Common.Models.Validators
{
    public class ResetPasswordModelValidator : AbstractValidator<ResetPasswordModel>
    {
        public ResetPasswordModelValidator()
        {
            RuleFor(x => x.UserId).NotNull().NotEmpty();
            RuleFor(x => x.Token).NotNull().NotEmpty();
            RuleFor(x => x.Password).Password();
            RuleFor(x => x.PasswordConfirm).NotNull().NotEmpty().Equal(x => x.Password)
                .WithMessage("The password and confirmation password do not match.");
        }
    }
}