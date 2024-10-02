using FluentValidation;
using Newtonsoft.Json.Linq;
using System;

namespace DynamicObjectAPI.Services.Validators
{
    public class OrderValidator : AbstractValidator<JObject>
    {
        public OrderValidator()
        {
            RuleFor(x => x["OrderDate"])
                .Must(BeValidDate).WithMessage("Order date must be a valid date")
                .When(x => x["OrderDate"] != null);

            RuleFor(x => (int?)x["CustomerId"])
                .NotEmpty().WithMessage("Customer ID is required")
                .GreaterThan(0).WithMessage("Customer ID must be greater than 0");

            When(x => x["OrderProducts"] != null && x["OrderProducts"].Type == JTokenType.Array, () => {
                RuleForEach(x => ((JArray)x["OrderProducts"]).Cast<JObject>())
                    .SetValidator(new OrderProductValidator())
                    .OverridePropertyName("OrderProducts");
            });
        }

        private bool BeValidDate(JToken value)
        {
            if (value == null) return true; // Null değer kabul edilebilir, çünkü Service'te otomatik olarak atanacak
            return DateTime.TryParse(value.ToString(), out _);
        }
    }
}