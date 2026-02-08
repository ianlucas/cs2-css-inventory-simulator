/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;

namespace InventorySimulator;

public static partial class Natives
{
    public static void CCSPlayer_WeaponServices_DropWeapon(nint thisPtr, nint weaponPtr)
    {
        VirtualFunction.CreateVoid<nint, nint, Vector?, Vector?>(thisPtr, GameData.GetOffset("CCSPlayer_WeaponServices::DropWeapon"))(thisPtr, weaponPtr, null, null);
    }
}
