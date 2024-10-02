using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicObjectAPI.Common.Exceptions
{
    public class RepositoryOperationException : Exception
    {
        public RepositoryOperationException() { }

        public RepositoryOperationException(string message)
            : base(message) { }

        public RepositoryOperationException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
