/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core;

namespace InventorySimulator;

public class CCSInventoryManager(nint handle)
{
    public nint Handle { get; set; } = handle;
    public bool IsValid => Handle != nint.Zero;

    public nint GetBaseItemForTeam(byte team, loadout_slot_t slot)
    {
        return Natives.CCSInventoryManager_GetBaseItemForTeam.Invoke(Handle, team, (int)slot);
    }

    public nint GetReferenceEconItem(ushort defIndex)
    {
        return Natives.CInventoryManager_GetReferenceEconItem.Invoke(Handle, defIndex, 0, 0);
    }
}
