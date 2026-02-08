/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Runtime.InteropServices;
using SwiftlyS2.Shared.Natives;

namespace InventorySimulator;

// Thanks to @samyycX.
public struct CGCClientSharedObjectCache(nint address) : INativeHandle
{
    public nint Address { get; set; } = address;
    public readonly bool IsValid => Address != 0;

    public readonly SOID_t Owner =>
        !IsValid
            ? throw new InvalidOperationException("Invalid cache.")
            : Marshal.PtrToStructure<SOID_t>(Address + Natives.CGCClientSharedObjectCache_m_Owner);
}
