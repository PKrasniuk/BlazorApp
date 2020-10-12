using BlazorApp.Common.Wrappers;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace BlazorApp.BLL.Infrastructure.Exceptions
{
    public class ApiException : Exception
    {
        public ApiException(string message, int statusCode = StatusCodes.Status500InternalServerError,
            string errorCode = "", string refLink = "") : base(message)
        {
            IsModelValidationError = false;
            StatusCode = statusCode;
            ReferenceErrorCode = errorCode;
            ReferenceDocumentLink = refLink;
        }

        public ApiException(IEnumerable<ValidationError> errors, int statusCode = StatusCodes.Status400BadRequest)
        {
            IsModelValidationError = true;
            StatusCode = statusCode;
            Errors = errors;
        }

        public ApiException(Exception ex, int statusCode = StatusCodes.Status500InternalServerError) : base(ex.Message)
        {
            IsModelValidationError = false;
            StatusCode = statusCode;
        }

        public int StatusCode { get; set; }

        public bool IsModelValidationError { get; set; }

        public IEnumerable<ValidationError> Errors { get; set; }

        public string ReferenceErrorCode { get; set; }

        public string ReferenceDocumentLink { get; set; }
    }
}