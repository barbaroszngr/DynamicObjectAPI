using FluentValidation;
using Newtonsoft.Json.Linq;

namespace DynamicObjectAPI.Services.Validators
{
    public class InvoiceLineValidator : AbstractValidator<JObject>
    {
        public InvoiceLineValidator()
        {
            RuleFor(x => (int?)x["Quantity"])
                .NotEmpty().WithMessage("Quantity is required")
                .GreaterThan(0).WithMessage("Quantity must be greater than 0");

            RuleFor(x => (decimal?)x["UnitPrice"])
                .NotEmpty().WithMessage("Unit price is required")
                .GreaterThan(0).WithMessage("Unit price must be greater than 0");

            RuleFor(x => (int?)x["ProductId"])
                .NotEmpty().WithMessage("Product ID is required")
                .GreaterThan(0).WithMessage("Product ID must be greater than 0");

            RuleFor(x => (int?)x["InvoiceId"])
                .NotEmpty().WithMessage("Invoice ID is required")
                .GreaterThan(0).WithMessage("Invoice ID must be greater than 0");
        }
    }
}