using BlazorApp.Common.Constants;
using FluentValidation;

namespace BlazorApp.Common.Models.Validators;

public static class RuleBuilderExtensions
{
    public static IRuleBuilder<T, string> Password<T>(this IRuleBuilder<T, string> ruleBuilder,
        int minimumLength = 6)
    {
        var options = ruleBuilder
            .NotNull()
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(minimumLength)
            .MaximumLength(FieldConstants.QuarterFieldLength)
            .Matches("[A-Z]")
            .Matches("[a-z]")
            .Matches("[0-9]")
            .Matches("[^a-zA-Z0-9]");
        return options;
    }
}