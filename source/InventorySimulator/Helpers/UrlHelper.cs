/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Reflection;
using System.Text.RegularExpressions;

namespace InventorySimulator;

public static partial class UrlHelper
{
    public static string FormatUrl(string format, string urlString)
    {
        if (!Uri.TryCreate(urlString, UriKind.Absolute, out var uri))
            return format;
        return PlaceholderPattern()
            .Replace(
                format,
                match =>
                {
                    var prop = uri.GetType()
                        .GetProperty(
                            match.Groups[1].Value,
                            BindingFlags.Public | BindingFlags.Instance
                        );
                    return prop?.GetValue(uri)?.ToString() ?? match.Value;
                }
            );
    }

    [GeneratedRegex(@"\{(\w+)\}")]
    private static partial Regex PlaceholderPattern();
}
