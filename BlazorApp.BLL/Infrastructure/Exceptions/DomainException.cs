using System;

namespace BlazorApp.BLL.Infrastructure.Exceptions
{
    public class DomainException : Exception
    {
        public DomainException(string description)
        {
            Description = description;
        }

        public DomainException(string description, string message) : base(message)
        {
            Description = description;
        }

        public DomainException(string description, string message, Exception innerException) : base(message,
            innerException)
        {
            Description = description;
        }

        public string Description { get; }
    }
}