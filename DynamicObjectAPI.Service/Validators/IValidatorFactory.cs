using FluentValidation;
using Newtonsoft.Json.Linq;

namespace DynamicObjectAPI.Services.Validators
{
    public interface IValidatorFactory
    {
        IValidator<JObject> GetValidator(string objectType);
    }
}
