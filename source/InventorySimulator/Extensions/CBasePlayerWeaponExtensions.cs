/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core;

namespace InventorySimulator;

public static class CBasePlayerWeaponExtensions
{
    public static string GetDesignerName(this CBasePlayerWeapon self)
    {
        var designerName = SchemaHelper.GetItemSchema()?.GetItemDefinition(self.AttributeManager.Item.ItemDefinitionIndex)?.DefinitionName ?? self.DesignerName;
        return ItemHelper.IsMeleeDesignerName(designerName) ? "weapon_knife" : designerName;
    }

    public static bool HasCustomItemID(this CBasePlayerWeapon self)
    {
        return self.AttributeManager.Item.ItemID >= CEconItemViewExtensions.MinimumCustomItemID;
    }
}
