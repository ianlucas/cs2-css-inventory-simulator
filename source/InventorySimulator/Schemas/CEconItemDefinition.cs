/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Runtime.InteropServices;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace InventorySimulator;

// Thanks @Kxnrl.
public class CEconItemDefinition(nint address) : INativeHandle
{
    public nint Address { get; set; } = address;
    public bool IsValid => Address != 0;

    public ushort DefIndex => (ushort)Marshal.ReadInt16(Address + 0x10);

    public string? DefinitionName
    {
        get
        {
            var ptr = Marshal.ReadIntPtr(Address + 0x260);
            return ptr != 0 ? Marshal.PtrToStringUTF8(ptr) : null;
        }
    }

    public loadout_slot_t DefaultLoadoutSlot => (loadout_slot_t)Marshal.ReadInt32(Address + 0x338);
}
