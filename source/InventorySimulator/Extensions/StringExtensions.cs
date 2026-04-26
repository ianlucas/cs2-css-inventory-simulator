/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Text.RegularExpressions;

namespace InventorySimulator;

public static partial class StringExtensions
{
    public static string StripColorTags(this string self)
    {
        return ColorTag().Replace(self, "");
    }

    [GeneratedRegex(@"\{.*?\}")]
    private static partial Regex ColorTag();
}
