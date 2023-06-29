using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BlazorApp.BLL.Infrastructure.Extensions;

public static class StringExtension
{
    public static bool IsValidJson(this string text)
    {
        text = text.Trim();

        if ((text.StartsWith("{") && text.EndsWith("}")) || (text.StartsWith("[") && text.EndsWith("]")))
            try
            {
                var obj = JToken.Parse(text);
                return true;
            }
            catch (JsonReaderException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }

        return false;
    }
}