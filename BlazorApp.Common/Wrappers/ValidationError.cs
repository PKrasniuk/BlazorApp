using Newtonsoft.Json;

namespace BlazorApp.Common.Wrappers;

public class ValidationError
{
    public ValidationError(string field, string message)
    {
        Field = field != string.Empty ? field : null;
        Message = message;
    }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Field { get; }

    public string Message { get; }
}