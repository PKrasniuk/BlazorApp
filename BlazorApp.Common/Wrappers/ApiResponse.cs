using BlazorApp.Common.Constants;
using BlazorApp.Common.Models;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace BlazorApp.Common.Wrappers
{
    [Serializable]
    [DataContract]
    public class ApiResponse
    {
        [JsonConstructor]
        public ApiResponse()
        {
        }

        public ApiResponse(int statusCode, string message = "", object result = null, ApiError apiError = null,
            string apiVersion = CommonConstants.ApiVersion, PaginationDetails<Guid> paginationDetails = null)
        {
            StatusCode = statusCode;
            Message = message;
            Result = result;
            ResponseException = apiError;
            Version = string.IsNullOrWhiteSpace(apiVersion)
                ? Assembly.GetEntryAssembly()?.GetName().Version.ToString()
                : apiVersion;
            IsError = false;
            PaginationDetails = paginationDetails;
        }

        public ApiResponse(int statusCode, ApiError apiError)
        {
            StatusCode = statusCode;
            Message = apiError.ExceptionMessage;
            ResponseException = apiError;
            IsError = true;
        }

        [DataMember] public string Version { get; set; }

        [DataMember] public int StatusCode { get; set; }

        [DataMember] public bool IsError { get; set; }

        [DataMember] public string Message { get; set; }

        [DataMember(EmitDefaultValue = false)] public ApiError ResponseException { get; set; }

        [DataMember(EmitDefaultValue = false)] public object Result { get; set; }

        [DataMember(EmitDefaultValue = false)] public PaginationDetails<Guid> PaginationDetails { get; set; }
    }
}