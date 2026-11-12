/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core;

namespace InventorySimulator;

public static partial class Natives
{
    private static readonly Lazy<int> _lazyCGameTrace_Size = new(() =>
        GameData.GetOffset("sizeof(CGameTrace)")
    );

    public static int CGameTrace_Size => _lazyCGameTrace_Size.Value;

    private static readonly Lazy<int> _lazyCGameTrace_m_vEndPos = new(() =>
        GameData.GetOffset("CGameTrace::m_vEndPos")
    );

    public static int CGameTrace_m_vEndPos => _lazyCGameTrace_m_vEndPos.Value;

    private static readonly Lazy<int> _lazyCGameTrace_m_vHitNormal = new(() =>
        GameData.GetOffset("CGameTrace::m_vHitNormal")
    );

    public static int CGameTrace_m_vHitNormal => _lazyCGameTrace_m_vHitNormal.Value;
}
