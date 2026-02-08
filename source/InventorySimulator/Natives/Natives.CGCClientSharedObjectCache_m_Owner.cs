/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core;

namespace InventorySimulator;

public static partial class Natives
{
    private static readonly Lazy<int> _lazyCGCClientSharedObjectCache_m_Owner = new(() => GameData.GetOffset("CGCClientSharedObjectCache::m_Owner"));

    public static int CGCClientSharedObjectCache_m_Owner => _lazyCGCClientSharedObjectCache_m_Owner.Value;
}
