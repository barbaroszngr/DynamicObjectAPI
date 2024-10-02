using FluentValidation;
using Newtonsoft.Json.Linq;

namespace DynamicObjectAPI.Services.Validators
{
    public class ProductValidator : AbstractValidator<JObject>
    {
        public ProductValidator()
        {
            RuleFor(x => (string)x["Name"])
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(255).WithMessage("Name cannot exceed 255 characters");

            RuleFor(x => (decimal?)x["Price"])
                .NotEmpty().WithMessage("Price is required")
                .GreaterThan(0).WithMessage("Price must be greater than 0");
        }
    }
}