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
}
