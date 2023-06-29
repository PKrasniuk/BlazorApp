using System;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using BlazorApp.Common.Constants;
using BlazorApp.Common.Models;
using Newtonsoft.Json;

namespace BlazorApp.Common.Wrappers;

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
        var version = Assembly.GetEntryAssembly()?.GetName().Version;
        if (version != null)
            Version = string.IsNullOrWhiteSpace(apiVersion)
                ? version.ToString()
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

    public static async Task<ApiResponse> ReturnApiResponse(HttpResponseMessage responseMessage)
    {
        return new ApiResponse((int)responseMessage.StatusCode, responseMessage.ReasonPhrase,
            await responseMessage.Content.ReadAsStringAsync());
    }
}