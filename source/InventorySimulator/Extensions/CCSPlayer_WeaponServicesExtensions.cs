/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core;

namespace InventorySimulator;

public static class CCSPlayer_WeaponServicesExtensions
{
    public static void DropWeapon(this CCSPlayer_WeaponServices self, CBasePlayerWeapon weapon)
    {
        Natives.CCSPlayer_WeaponServices_DropWeapon(self.Handle, weapon.Handle);
    }
}
