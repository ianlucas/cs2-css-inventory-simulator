/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Runtime.InteropServices;
using CounterStrikeSharp.API.Core;

namespace InventorySimulator;

// Thanks to @samyycX.
public class CCSPlayerInventory(nint handle)
{
    public nint Handle { get; set; } = handle;
    public bool IsValid => Handle != nint.Zero && SOCache.IsValid;
    public ulong SteamID => SOCache.Owner.SteamID;

    public CGCClientSharedObjectCache SOCache =>
        new(Marshal.ReadIntPtr(Handle + Natives.CCSPlayerInventory_m_pSOCache));

    public nint GetItemInLoadout(byte team, loadout_slot_t slot)
    {
        return Natives.CCSPlayerInventory_GetItemInLoadout.Invoke(Handle, team, (int)slot);
    }

    // Only the player's own items, the game's stock weapons have no item ID.
    public nint FindWeaponInLoadout(byte team, ushort def)
    {
        for (
            var slot = loadout_slot_t.LOADOUT_SLOT_FIRST_WHEEL_WEAPON;
            slot <= loadout_slot_t.LOADOUT_SLOT_LAST_WHEEL_WEAPON;
            slot++
        )
        {
            // Unhooked, so it reads the player's game loadout.
            var ptr = Natives.CCSPlayerInventory_GetItemInLoadout.Invoke(
                Handle,
                team,
                (int)slot,
                bypasshook: true
            );
            if (ptr == nint.Zero)
                continue;
            var itemView = new CEconItemView(ptr);
            if (itemView.ItemID != 0 && itemView.ItemDefinitionIndex == def)
                return ptr;
        }
        return nint.Zero;
    }

    public void SendInventoryUpdateEvent()
    {
        Natives.CCSPlayerInventory_SendInventoryUpdateEvent.Invoke(Handle);
    }
}
