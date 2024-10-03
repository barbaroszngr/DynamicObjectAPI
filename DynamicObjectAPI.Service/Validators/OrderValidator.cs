using FluentValidation;
using Newtonsoft.Json.Linq;
using System;

namespace DynamicObjectAPI.Services.Validators
{
    public class OrderValidator : AbstractValidator<JObject>
    {
        public OrderValidator()
        {
            RuleFor(x => (int?)x["CustomerId"])
                .NotEmpty().WithMessage("Customer ID is required")
                .GreaterThan(0).WithMessage("Customer ID must be greater than 0");

            RuleFor(x => x["OrderProducts"])
                .NotEmpty().WithMessage("Order must have at least one product");

            When(x => x["OrderProducts"] != null && x["OrderProducts"].Type == JTokenType.Array, () => {
                RuleForEach(x => ((JArray)x["OrderProducts"]).Cast<JObject>())
                    .SetValidator(new OrderProductValidator())
                    .OverridePropertyName("OrderProducts");
            });
        }
    }

}