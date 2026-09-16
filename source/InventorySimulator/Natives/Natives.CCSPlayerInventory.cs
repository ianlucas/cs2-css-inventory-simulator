/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;

namespace InventorySimulator;

public static partial class Natives
{
    public static readonly MemoryFunctionWithReturn<
        nint,
        int,
        int,
        nint
    > CCSPlayerInventory_GetItemInLoadout = new(
        GameData.GetSignature("CCSPlayerInventory::GetItemInLoadout")
    );

    public static readonly MemoryFunctionWithReturn<
        nint,
        nint
    > CCSPlayerInventory_SendInventoryUpdateEvent = new(
        GameData.GetSignature("CCSPlayerInventory::SendInventoryUpdateEvent")
    );

    private static readonly Lazy<int> _lazyCCSPlayerInventory_m_pSOCache = new(() =>
        GameData.GetOffset("CCSPlayerInventory::m_pSOCache")
    );

    public static int CCSPlayerInventory_m_pSOCache => _lazyCCSPlayerInventory_m_pSOCache.Value;
}
