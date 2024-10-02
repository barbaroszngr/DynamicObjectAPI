using System;

namespace DynamicObjectAPI.Common.Exceptions
{
    public class NotFoundException : Exception
    {
        public string ObjectType { get; }
        public object Id { get; }

        public NotFoundException(string objectType, object id)
            : base($"{objectType} with id {id} was not found.")
        {
            ObjectType = objectType;
            Id = id;
        }
    }
}