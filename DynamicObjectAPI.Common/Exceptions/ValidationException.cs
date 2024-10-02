using System;
using System.Collections.Generic;

namespace DynamicObjectAPI.Common.Exceptions
{
    public class ValidationException : Exception
    {
        public IList<string> Errors { get; }

        public ValidationException(IList<string> errors)
            : base("One or more validation errors occurred.")
        {
            Errors = errors ?? new List<string>();
        }

        public ValidationException(string error)
            : this(new List<string> { error })
        {
        }
    }
}