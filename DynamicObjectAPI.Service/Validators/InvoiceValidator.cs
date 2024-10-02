using FluentValidation;
using Newtonsoft.Json.Linq;
using System;

namespace DynamicObjectAPI.Services.Validators
{
    public class InvoiceValidator : AbstractValidator<JObject>
    {
        public InvoiceValidator()
        {
            RuleFor(x => (string)x["InvoiceDate"])
                .NotEmpty().WithMessage("Invoice date is required")
                .Must(BeValidDate).WithMessage("Invoice date must be a valid date");

            RuleFor(x => (decimal?)x["TotalAmount"])
                .NotEmpty().WithMessage("Total amount is required")
                .GreaterThan(0).WithMessage("Total amount must be greater than 0");

            RuleFor(x => (int?)x["CustomerId"])
                .NotEmpty().WithMessage("Customer ID is required")
                .GreaterThan(0).WithMessage("Customer ID must be greater than 0");

            When(x => x["InvoiceLines"] != null && x["InvoiceLines"].Type == JTokenType.Array, () => {
                RuleFor(x => ((JArray)x["InvoiceLines"]))
                    .NotEmpty().WithMessage("Invoice must have at least one line item");
                RuleForEach(x => ((JArray)x["InvoiceLines"]).Cast<JObject>())
                    .SetValidator(new InvoiceLineValidator());
            });
        }

        private bool BeValidDate(string value) => DateTime.TryParse(value, out _);
    }
}