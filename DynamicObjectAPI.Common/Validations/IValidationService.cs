using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicObjectAPI.Common.Validations
{
    public interface IValidationService
    {
        Task ValidateAsync(string objectType, string data);
    }
}
