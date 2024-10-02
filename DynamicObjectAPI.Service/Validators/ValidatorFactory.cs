using FluentValidation;
using Newtonsoft.Json.Linq;

namespace DynamicObjectAPI.Services.Validators
{
    public class ValidatorFactory : IValidatorFactory
    {
        public IValidator<JObject> GetValidator(string objectType)
        {
            return objectType.ToLower() switch
            {
                "customer" => new CustomerValidator(),
                "invoice" => new InvoiceValidator(),
                "invoiceline" => new InvoiceLineValidator(),
                "order" => new OrderValidator(),
                "orderproduct" => new OrderProductValidator(),
                "product" => new ProductValidator(),
                _ => null,
            };
        }
    }
}