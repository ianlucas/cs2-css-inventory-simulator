/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core.Translations;

namespace InventorySimulator;

public static class InventorySimulatorCtx
{
    public static string GetChatPrefix(bool stripColors = false)
    {
        var prefix = ConVars.ChatPrefix.Value;
        if (prefix != "")
            return $"{(stripColors
            ? prefix.StripColorTags()
            : prefix.ReplaceColorTags())} ";
        return "";
    }
}
