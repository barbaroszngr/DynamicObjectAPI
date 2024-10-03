using FluentValidation;
using Newtonsoft.Json.Linq;

namespace DynamicObjectAPI.Services.Validators
{
    public class OrderProductValidator : AbstractValidator<JObject>
    {
        public OrderProductValidator()
        {
            RuleFor(x => (int?)x["Quantity"])
                .NotEmpty().WithMessage("Quantity is required")
                .GreaterThan(0).WithMessage("Quantity must be greater than 0");

            RuleFor(x => (int?)x["ProductId"])
                .NotEmpty().WithMessage("Product ID is required")
                .GreaterThan(0).WithMessage("Product ID must be greater than 0");

            
        }
    }

}