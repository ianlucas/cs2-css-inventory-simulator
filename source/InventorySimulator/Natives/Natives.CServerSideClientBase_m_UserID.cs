/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core;

namespace InventorySimulator;

public static partial class Natives
{
    private static readonly Lazy<int> _lazyCServerSideClientBase_m_UserID = new(() => GameData.GetOffset("CServerSideClientBase::m_UserID"));

    public static int CServerSideClientBase_m_UserID => _lazyCServerSideClientBase_m_UserID.Value;
}
