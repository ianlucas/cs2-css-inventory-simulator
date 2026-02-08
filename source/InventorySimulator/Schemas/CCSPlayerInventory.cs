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
    public bool IsValid => Handle != 0 && SOCache.IsValid;
    public ulong SteamID => SOCache.Owner.SteamID;

    public CGCClientSharedObjectCache SOCache =>
        new(Marshal.ReadIntPtr(Handle + Natives.CCSPlayerInventory_m_pSOCache));

    public nint GetItemInLoadout(byte team, loadout_slot_t slot)
    {
        return Natives.CCSPlayerInventory_GetItemInLoadout.Invoke(Handle, team, (int)slot);
    }

    public void SendInventoryUpdateEvent()
    {
        Natives.CCSPlayerInventory_SendInventoryUpdateEvent.Invoke(Handle);
    }
}
