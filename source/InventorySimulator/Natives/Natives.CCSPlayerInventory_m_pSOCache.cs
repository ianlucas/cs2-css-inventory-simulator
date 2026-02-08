/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core;

namespace InventorySimulator;

public static partial class Natives
{
    private static readonly Lazy<int> _lazyCCSPlayerInventory_m_pSOCache = new(() =>
        GameData.GetOffset("CCSPlayerInventory::m_pSOCache")
    );

    public static int CCSPlayerInventory_m_pSOCache => _lazyCCSPlayerInventory_m_pSOCache.Value;
}
