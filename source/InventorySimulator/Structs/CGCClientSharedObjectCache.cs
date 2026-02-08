/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Runtime.InteropServices;

namespace InventorySimulator;

// Thanks to @samyycX.
public struct CGCClientSharedObjectCache(nint handle)
{
    public nint Handle { get; set; } = handle;
    public readonly bool IsValid => Handle != 0;

    public readonly SOID_t Owner =>
        !IsValid ? throw new InvalidOperationException("Invalid cache.") : Marshal.PtrToStructure<SOID_t>(Handle + Natives.CGCClientSharedObjectCache_m_Owner);
}
