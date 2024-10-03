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

        }
    }

}