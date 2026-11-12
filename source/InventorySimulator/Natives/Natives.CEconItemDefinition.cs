/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core;

namespace InventorySimulator;

public static partial class Natives
{
    private static readonly Lazy<int> _lazyCEconItemDefinition_m_nDefIndex = new(() =>
        GameData.GetOffset("CEconItemDefinition::m_nDefIndex")
    );

    public static int CEconItemDefinition_m_nDefIndex => _lazyCEconItemDefinition_m_nDefIndex.Value;

    private static readonly Lazy<int> _lazyCEconItemDefinition_m_pszDefinitionName = new(() =>
        GameData.GetOffset("CEconItemDefinition::m_pszDefinitionName")
    );

    public static int CEconItemDefinition_m_pszDefinitionName =>
        _lazyCEconItemDefinition_m_pszDefinitionName.Value;

    private static readonly Lazy<int> _lazyCEconItemDefinition_m_iDefaultLoadoutSlot = new(() =>
        GameData.GetOffset("CEconItemDefinition::m_iDefaultLoadoutSlot")
    );

    public static int CEconItemDefinition_m_iDefaultLoadoutSlot =>
        _lazyCEconItemDefinition_m_iDefaultLoadoutSlot.Value;
}
