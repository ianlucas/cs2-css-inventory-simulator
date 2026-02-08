/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core;

namespace InventorySimulator;

public static partial class Natives
{
    private static readonly Lazy<int> _lazyCCSPlayerController_InventoryServices_m_pInventory = new(
        () =>
            GameData.GetOffset("CCSPlayerController_InventoryServices::m_pInventory")
    );

    public static int CCSPlayerController_InventoryServices_m_pInventory =>
        _lazyCCSPlayerController_InventoryServices_m_pInventory.Value;
}
