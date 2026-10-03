/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;

namespace InventorySimulator;

public static partial class Natives
{
    public static readonly MemoryFunctionWithReturn<nint, nint, bool> CChicken_InitPet = new(
        GameData.GetSignature("CChicken::InitPet")
    );

    private static readonly Lazy<int> _lazyCChicken_m_bCanRoam = new(() =>
        GameData.GetOffset("CChicken::m_bCanRoam")
    );

    public static int CChicken_m_bCanRoam => _lazyCChicken_m_bCanRoam.Value;
}
