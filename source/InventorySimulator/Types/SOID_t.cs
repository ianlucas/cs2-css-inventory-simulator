/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Runtime.InteropServices;

namespace InventorySimulator;

// Thanks to @samyycX.
[StructLayout(LayoutKind.Sequential)]
public readonly struct SOID_t
{
    private readonly ulong m_id;
    public readonly ulong SteamID => m_id;
}
