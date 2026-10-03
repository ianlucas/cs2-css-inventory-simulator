/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace InventorySimulator;

public class PlayerInventory(EquippedV5Response data)
{
    private readonly EquippedV5Response _data = data;
    public Dictionary<byte, InventoryItem> Agents => _data.Agents;
    public InventoryItem? MusicKit => _data.MusicKit;
    public InventoryItem? Graffiti => _data.Graffiti;
    public InventoryItem? Pet => _data.Pet;

    public static PlayerInventory Empty() => new(new());

    public IEnumerable<InventoryItem> GetAllWeapons() =>
        _data.Knives.Values.Concat(_data.CTWeapons.Values).Concat(_data.TWeapons.Values);

    public void InitializeWearOverrides()
    {
        foreach (var item in GetAllWeapons())
            item.WearOverride = WearRegistry.Resolve(item);
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
        if (slot == loadout_slot_t.LOADOUT_SLOT_PET)
            return _data.Pet;
        return null;
    }

    public void ClearGraffiti()
    {
        _data.Graffiti = null;
    }
}
