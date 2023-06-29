using BlazorApp.Common.Constants;
using FluentValidation;

namespace BlazorApp.Common.Models.Validators;

public class UserInfoModelValidator : AbstractValidator<UserInfoModel>
{
    public UserInfoModelValidator()
    {
        RuleFor(x => x.UserId).NotNull().NotEmpty();
        RuleFor(x => x.UserName).NotNull().NotEmpty().MaximumLength(FieldConstants.QuarterFieldLength);
        RuleFor(x => x.Email).NotNull().NotEmpty().EmailAddress();
        RuleFor(x => x.FirstName).NotNull().NotEmpty().MaximumLength(FieldConstants.QuarterFieldLength);
        RuleFor(x => x.LastName).NotNull().NotEmpty().MaximumLength(FieldConstants.QuarterFieldLength);
    }
}