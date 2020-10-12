using FluentValidation;

namespace BlazorApp.Common.Models.Validators
{
    public class ConfirmEmailModelValidator : AbstractValidator<ConfirmEmailModel>
    {
        public ConfirmEmailModelValidator()
        {
            RuleFor(x => x.UserId).NotNull().NotEmpty();
            RuleFor(x => x.Token).NotNull().NotEmpty();
        }
    }
}