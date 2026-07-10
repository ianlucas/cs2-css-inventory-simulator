/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Numerics;
using System.Runtime.InteropServices;

namespace InventorySimulator;

// Thanks @Nukoooo.
[StructLayout(LayoutKind.Explicit, Size = 0xC0)]
public struct CGameTrace
{
    [FieldOffset(0x84)]
    public Vector3 EndPos;

    [FieldOffset(0x90)]
    public Vector3 HitNormal;
}
