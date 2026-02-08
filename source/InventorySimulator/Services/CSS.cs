/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core;

namespace InventorySimulator;

public static class CSS
{
    public static BasePlugin Plugin { get; set; } = null!;

    public static void Initialize(BasePlugin plugin)
    {
        Plugin = plugin;
    }
}
