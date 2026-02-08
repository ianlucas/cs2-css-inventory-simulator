/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Runtime.InteropServices;
using CounterStrikeSharp.API.Core;

namespace InventorySimulator;

// Thanks @Kxnrl.
public class CEconItemDefinition(nint handle)
{
    public nint Handle { get; set; } = handle;
    public bool IsValid => Handle != 0;

    public ushort DefIndex => (ushort)Marshal.ReadInt16(Handle + 0x10);

    public string? DefinitionName
    {
        get
        {
            var ptr = Marshal.ReadIntPtr(Handle + 0x260);
            return ptr != 0 ? Marshal.PtrToStringUTF8(ptr) : null;
        }
    }

    public loadout_slot_t DefaultLoadoutSlot => (loadout_slot_t)Marshal.ReadInt32(Handle + 0x338);
}
