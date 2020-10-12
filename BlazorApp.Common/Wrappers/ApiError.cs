using System.Collections.Generic;

namespace BlazorApp.Common.Wrappers
{
    public class ApiError
    {
        public ApiError(string message)
        {
            ExceptionMessage = message;
            IsError = true;
        }

        public ApiError(string message, IEnumerable<ValidationError> validationErrors)
        {
            ExceptionMessage = message;
            ValidationErrors = validationErrors;
        }

        public bool IsError { get; set; }

        public string ExceptionMessage { get; set; }

        public string Details { get; set; }

        public string ReferenceErrorCode { get; set; }

        public string ReferenceDocumentLink { get; set; }

        public IEnumerable<ValidationError> ValidationErrors { get; set; }
    }
}