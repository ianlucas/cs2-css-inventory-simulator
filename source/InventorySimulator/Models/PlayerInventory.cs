/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace InventorySimulator;

public class PlayerInventory(EquippedV4Response data)
{
    private readonly EquippedV4Response _data = data;
    public Dictionary<byte, InventoryItem> Agents => _data.Agents;
    public InventoryItem? MusicKit => _data.MusicKit;
    public InventoryItem? Graffiti => _data.Graffiti;

    public Dictionary<(int paint, float wear), (ushort def, string stickers)> WeaponWearCache = [];

    public static PlayerInventory Empty() => new(new());

    public void InitializeWearOverrides()
    {
        foreach (var knife in _data.Knives.Values)
            knife.WearOverride = GetWeaponWear(knife);
        foreach (var weapon in _data.CTWeapons.Values)
            weapon.WearOverride = GetWeaponWear(weapon);
        foreach (var weapon in _data.TWeapons.Values)
            weapon.WearOverride = GetWeaponWear(weapon);
    }

    public InventoryItem? GetKnife(byte team, bool fallback)
    {
        if (_data.Knives.TryGetValue(team, out var knife))
            return knife;
        if (fallback && _data.Knives.TryGetValue(TeamHelper.ToggleTeam(team), out knife))
            return knife;
        return null;
    }

    public Dictionary<ushort, InventoryItem> GetWeapons(byte team)
    {
        return (CsTeam)team == CsTeam.Terrorist ? _data.TWeapons : _data.CTWeapons;
    }

    public InventoryItem? GetWeapon(byte team, ushort def, bool fallback)
    {
        if (GetWeapons(team).TryGetValue(def, out var weapon))
            return weapon;
        if (fallback && GetWeapons(TeamHelper.ToggleTeam(team)).TryGetValue(def, out weapon))
            return weapon;
        return null;
    }

    public InventoryItem? GetGloves(byte team, bool fallback)
    {
        if (_data.Gloves.TryGetValue(team, out var glove))
            return glove;
        if (fallback && _data.Gloves.TryGetValue(TeamHelper.ToggleTeam(team), out glove))
            return glove;
        return null;
    }

    // It looks like CS2's client caches the weapon materials based on wear and seed. Until we figure out
    // the proper way to force a material update, we need to ensure that every paint has a unique wear so
    // that: 1) it gets regenerated, and 2) there are no rendering issues. As a drawback, every time
    // players use !ws, their weapon's wear will decay, but I think it's a good trade-off since it also
    // forces the sticker to regenerate. This approach is based on workarounds by @stefanx111 and @bklol.
    private float GetWeaponWear(InventoryItem item)
    {
        if (item is not { Def: not null, Paint: not null, Wear: not null, Stickers: not null })
            return 0;
        var def = item.Def.Value;
        var paint = item.Paint.Value;
        var wear = item.Wear.Value;
        var stickers = string.Join("_", item.Stickers.Select(s => s.Def));
        while (
            WeaponWearCache.TryGetValue((paint, wear), out var cached)
            && (cached.def != def || cached.stickers != stickers)
        )
            wear += 0.001f;
        WeaponWearCache[(paint, wear)] = (def, stickers);
        return wear;
    }

    public InventoryItem? GetItemForSlot(
        byte team,
        loadout_slot_t slot,
        ushort def,
        bool fallback,
        int minModels = 0
    )
    {
        if (
            slot >= loadout_slot_t.LOADOUT_SLOT_MELEE
            && slot <= loadout_slot_t.LOADOUT_SLOT_EQUIPMENT5
        )
        {
            return slot == loadout_slot_t.LOADOUT_SLOT_MELEE
                ? GetKnife(team, fallback)
                : GetWeapon(team, def, fallback);
        }
        if (slot == loadout_slot_t.LOADOUT_SLOT_CLOTHING_CUSTOMPLAYER)
        {
            if (minModels > 0)
                return team == (byte)CsTeam.Terrorist
                    ? new InventoryItem { Def = 5036 }
                    : new InventoryItem { Def = 5037 };
            if (_data.Agents.TryGetValue(team, out var item))
                return item;
            return null;
        }
        if (slot == loadout_slot_t.LOADOUT_SLOT_CLOTHING_HANDS)
        {
            return GetGloves(team, fallback);
        }
        if (slot == loadout_slot_t.LOADOUT_SLOT_FLAIR0)
            return _data.Collectible;
        if (slot == loadout_slot_t.LOADOUT_SLOT_MUSICKIT)
            return _data.MusicKit;
        return null;
    }
}
