/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;

namespace InventorySimulator;

public static partial class Natives
{
    public static readonly MemoryFunctionVoid<nint> CServerSideClientBase_ActivatePlayer = new(
        GameData.GetSignature("CServerSideClientBase::ActivatePlayer"),
        Addresses.EnginePath
    );

    private static readonly Lazy<int> _lazyCServerSideClientBase_m_UserID = new(() =>
        GameData.GetOffset("CServerSideClientBase::m_UserID")
    );

    public static int CServerSideClientBase_m_UserID => _lazyCServerSideClientBase_m_UserID.Value;
}
