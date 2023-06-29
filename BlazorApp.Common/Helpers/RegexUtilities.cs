﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BlazorApp.Common.Helpers;

public static class RegexUtilities
{
    public static readonly Regex StringInterpolationHelper =
        new("(?<=[\\ \"]{0,1}{)(.+?)(?=})(?:[\\ \\.\"]{0,2}|$)", RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(100));

    private static readonly Regex DirtyXmlPropertyParserRegex =
        new("(?<=<property key=\')(?<key>[a-zA-Z0-9\\- :]+?)(?:'>)(?<value>[^><]+?)(?=<\\/property>)",
            RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

    public static Dictionary<string, string> DirtyXmlPropertyParser(string input)
    {
        var propertyList = new Dictionary<string, string>();
        var xmlMatch = DirtyXmlPropertyParserRegex.Match(input);
        while (xmlMatch.Success)
        {
            propertyList.Add(xmlMatch.Groups["key"].Value, xmlMatch.Groups["value"].Value);
            xmlMatch = xmlMatch.NextMatch();
        }

        return propertyList;
    }
}