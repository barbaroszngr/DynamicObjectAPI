using FluentValidation;
using Newtonsoft.Json.Linq;

namespace DynamicObjectAPI.Services.Validators
{
    public class CustomerValidator : AbstractValidator<JObject>
    {
        public CustomerValidator()
        {
            RuleFor(x => (string)x["Name"])
                .NotEmpty().WithMessage("Name is required")
                .MinimumLength(3).WithMessage("Name must be at least 3 characters long")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

            RuleFor(x => (string)x["Email"])
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("A valid email is required")
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");

            When(x => x["Invoices"] != null && x["Invoices"].Type == JTokenType.Array, () => {
                RuleForEach(x => ((JArray)x["Invoices"]).Cast<JObject>())
                    .SetValidator(new InvoiceValidator())
                    .OverridePropertyName("Invoices");
            });

            When(x => x["Orders"] != null && x["Orders"].Type == JTokenType.Array, () => {
                RuleForEach(x => ((JArray)x["Orders"]).Cast<JObject>())
                    .SetValidator(new OrderValidator())
                    .OverridePropertyName("Orders");
            });
        }
    }
}