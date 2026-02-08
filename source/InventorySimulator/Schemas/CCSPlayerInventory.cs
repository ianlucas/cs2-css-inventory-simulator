/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Runtime.InteropServices;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace InventorySimulator;

// Thanks to @samyycX.
public class CCSPlayerInventory(nint address) : INativeHandle
{
    public nint Address { get; set; } = address;
    public bool IsValid => Address != 0 && SOCache.IsValid;
    public ulong SteamID => SOCache.Owner.SteamID;

    public CGCClientSharedObjectCache SOCache =>
        new(Marshal.ReadIntPtr(Address + Natives.CCSPlayerInventory_m_pSOCache));

    public nint GetItemInLoadout(byte team, loadout_slot_t slot)
    {
        return Natives.CCSPlayerInventory_GetItemInLoadout.Call(Address, team, (int)slot);
    }

    public void SendInventoryUpdateEvent()
    {
        Natives.CCSPlayerInventory_SendInventoryUpdateEvent.Call(Address);
    }
}
