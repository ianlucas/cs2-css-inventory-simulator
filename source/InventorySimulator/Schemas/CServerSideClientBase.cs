/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Runtime.InteropServices;

namespace InventorySimulator;

public class CServerSideClientBase(nint handle)
{
    public nint Handle { get; set; } = handle;
    public bool IsValid => Handle != nint.Zero;
    public ushort UserID =>
        (ushort)Marshal.ReadInt16(Handle + Natives.CServerSideClientBase_m_UserID);
}
