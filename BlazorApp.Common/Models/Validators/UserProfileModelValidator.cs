using BlazorApp.Common.Constants;
using FluentValidation;

namespace BlazorApp.Common.Models.Validators;

public class UserProfileModelValidator : AbstractValidator<UserProfileModel>
{
    public UserProfileModelValidator()
    {
        RuleFor(x => x.Id).NotNull();
        RuleFor(x => x.UserId).NotNull().NotEmpty();
        RuleFor(x => x.LastPageVisited).NotNull().NotEmpty().MaximumLength(FieldConstants.BigFieldLength);
        RuleFor(x => x.IsNavOpen).NotNull();
        RuleFor(x => x.IsNavMinified).NotNull();
        RuleFor(x => x.Count).NotNull();
        RuleFor(x => x.LastUpdatedDate).NotNull();
    }
}