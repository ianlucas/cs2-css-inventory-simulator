/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Numerics;
using System.Runtime.InteropServices;

namespace InventorySimulator;

// Thanks @Nukoooo.
public struct CGameTrace(nint handle)
{
    public nint Handle { get; set; } = handle;
    public readonly bool IsValid => Handle != nint.Zero;

    public readonly Vector3 EndPos =>
        Marshal.PtrToStructure<Vector3>(Handle + Natives.CGameTrace_m_vEndPos);

    public readonly Vector3 HitNormal =>
        Marshal.PtrToStructure<Vector3>(Handle + Natives.CGameTrace_m_vHitNormal);
}
