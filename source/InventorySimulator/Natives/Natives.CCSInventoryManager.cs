/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;

namespace InventorySimulator;

public static partial class Natives
{
    public static readonly nint CSInventoryManager = GetGameSystemByName("CCSInventoryManager");

    public static readonly MemoryFunctionWithReturn<
        nint,
        int,
        int,
        nint
    > CCSInventoryManager_GetBaseItemForTeam = new(
        GameData.GetSignature("CCSInventoryManager::GetBaseItemForTeam")
    );
}
