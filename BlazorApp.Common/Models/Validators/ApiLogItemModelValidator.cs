using BlazorApp.Common.Constants;
using FluentValidation;

namespace BlazorApp.Common.Models.Validators
{
    public class ApiLogItemModelValidator : AbstractValidator<ApiLogItemModel>
    {
        public ApiLogItemModelValidator()
        {
            RuleFor(x => x.Id).NotNull();
            RuleFor(x => x.RequestTime).NotNull();
            RuleFor(x => x.ResponseMillis).NotNull();
            RuleFor(x => x.StatusCode).NotNull();
            RuleFor(x => x.Method).NotNull().NotEmpty();
            RuleFor(x => x.Path).NotNull().NotEmpty().MaximumLength(FieldConstants.BigFieldLength);
            RuleFor(x => x.QueryString).MaximumLength(FieldConstants.BaseFieldLength);
            RuleFor(x => x.RequestBody).MinimumLength(FieldConstants.BaseFieldLength);
            RuleFor(x => x.ResponseBody).MinimumLength(FieldConstants.BaseFieldLength);
            RuleFor(x => x.IpAddress).MinimumLength(FieldConstants.BaseFieldLength);
            RuleFor(x => x.ApplicationUserId).NotEmpty();
        }
    }
}