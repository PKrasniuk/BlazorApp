using BlazorApp.Common.Constants;
using FluentValidation;

namespace BlazorApp.Common.Models.Validators
{
    public class TodoModelValidator : AbstractValidator<TodoModel>
    {
        public TodoModelValidator()
        {
            RuleFor(x => x.Id);
            RuleFor(x => x.Title).NotNull().NotEmpty().MaximumLength(FieldConstants.HalfFieldLength);
            RuleFor(x => x.IsCompleted).NotNull();
        }
    }
}