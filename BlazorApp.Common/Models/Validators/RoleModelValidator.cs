using BlazorApp.Common.Constants;
using FluentValidation;

namespace BlazorApp.Common.Models.Validators;

public class RoleModelValidator : AbstractValidator<RoleModel>
{
    public RoleModelValidator()
    {
        RuleFor(x => x.Name).NotNull().NotEmpty().MaximumLength(FieldConstants.QuarterFieldLength);
    }
}