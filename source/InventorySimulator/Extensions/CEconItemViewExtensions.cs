/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core;

namespace InventorySimulator;

public static class CEconItemViewExtensions
{
    public static readonly ulong MinimumCustomItemID = 65155030971;
    private static ulong NextItemId = MinimumCustomItemID;

    public static void ApplyAttributes(this CEconItemView self, InventoryItem item, loadout_slot_t? slot, ulong? steamId)
    {
        var isMelee = slot == loadout_slot_t.LOADOUT_SLOT_MELEE;
        self.Initialized = true;
        if (item.Def != null)
            self.ItemDefinitionIndex = item.Def.Value;
        var itemId = NextItemId++;
        self.ItemID = itemId;
        self.ItemIDLow = (uint)(itemId & 0xFFFFFFFF);
        self.ItemIDHigh = (uint)(itemId >> 32);
        if (steamId != null)
            self.AccountID = new CSteamID(steamId.Value).GetAccountID().m_AccountID;
        if (isMelee)
            self.EntityQuality = 3;
        else
            self.EntityQuality = item.Stattrak >= 0 ? 9 : 4;
        if (item.Nametag != null)
            self.CustomName = item.Nametag;
        var customAttrs = item.GetAttributes();
        var attrs = self.NetworkedDynamicAttributes;
        attrs.Attributes.RemoveAll();
        foreach (var (attributeName, value) in customAttrs)
            attrs.SetOrAddAttribute(attributeName, value);
    }
}
