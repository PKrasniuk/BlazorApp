using BlazorApp.Common.Constants;
using FluentValidation;

namespace BlazorApp.Common.Models.Validators
{
    public class LoginModelValidator : AbstractValidator<LoginModel>
    {
        public LoginModelValidator()
        {
            RuleFor(x => x.UserName).NotNull().NotEmpty().MaximumLength(FieldConstants.QuarterFieldLength);
            RuleFor(x => x.Password).NotNull().NotEmpty().MaximumLength(FieldConstants.QuarterFieldLength);
        }
    }
}