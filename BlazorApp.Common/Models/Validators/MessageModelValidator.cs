using BlazorApp.Common.Constants;
using FluentValidation;

namespace BlazorApp.Common.Models.Validators;

public class MessageModelValidator : AbstractValidator<MessageModel>
{
    public MessageModelValidator()
    {
        RuleFor(x => x.Id).NotNull();
        RuleFor(x => x.UserName).NotNull().NotEmpty().MaximumLength(FieldConstants.BaseFieldLength);
        RuleFor(x => x.Text).NotNull().NotEmpty().MaximumLength(FieldConstants.BigFieldLength);
        RuleFor(x => x.When).NotNull();
        RuleFor(x => x.Mine).NotNull();
        RuleFor(x => x.UserId).NotNull().NotEmpty();
    }
}