/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Concurrent;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace InventorySimulator;

public static class CCSPlayerControllerExtensions
{
    private static readonly ConcurrentDictionary<uint, CCSPlayerControllerState> _controllerStateManager = [];

    public static CCSPlayerControllerState GetState(this CCSPlayerController self)
    {
        return _controllerStateManager.GetOrAdd(self.Index, _ => new(self.SteamID));
    }

    public static void Revalidate(this CCSPlayerController self)
    {
        if (self.GetState().SteamID != self.SteamID)
            self.RemoveState();
    }

    public static void RemoveState(this CCSPlayerController self)
    {
        var controllerState = self.GetState();
        controllerState.DisposeUseCmdTimer();
        controllerState.ClearEconItemView();
        _controllerStateManager.TryRemove(self.Index, out var _);
    }

    public static bool IsUseCmdBusy(this CCSPlayerController self)
    {
        if (self.PlayerPawn.Value?.IsBuyMenuOpen == true)
            return true;
        if (self.PlayerPawn.Value?.IsDefusing == true)
            return true;
        var weapon = self.PlayerPawn.Value?.WeaponServices?.ActiveWeapon.Value;
        if (weapon?.DesignerName != "weapon_c4")
            return false;
        var c4 = weapon.As<CC4>();
        return c4.IsPlantingViaUse;
    }

    public static void RegiveAgent(this CCSPlayerController self, PlayerInventory inventory, PlayerInventory? oldInventory)
    {
        if (ConVars.MinModels.Value > 0)
            return;
        var pawn = self.PlayerPawn.Value;
        if (pawn == null)
            return;
        var teamNum = self.TeamNum;
        var item = inventory.Agents.TryGetValue(teamNum, out var a) ? a : null;
        var oldItem = oldInventory != null && oldInventory.Agents.TryGetValue(teamNum, out a) ? a : null;
        if (oldItem == item)
            return;
        pawn.SetModelFromLoadout();
        pawn.SetModelFromClass();
        pawn.AcceptInput("SetBodygroup", value: "default_gloves,1");
    }

    public static void RegiveGloves(this CCSPlayerController self, PlayerInventory inventory, PlayerInventory? oldInventory)
    {
        var pawn = self.PlayerPawn.Value;
        var itemServices = pawn?.ItemServices?.As<CCSPlayer_ItemServices>();
        if (pawn == null || itemServices == null)
            return;
        var isFallbackTeam = ConVars.IsFallbackTeam.Value;
        var teamNum = self.TeamNum;
        var item = inventory.GetGloves(teamNum, isFallbackTeam);
        var oldItem = oldInventory?.GetGloves(teamNum, isFallbackTeam);
        if (oldItem == item)
            return;
        // Workaround by @daffyyyy.
        var model = pawn.CBodyComponent?.SceneNode?.GetSkeletonInstance()?.ModelState.ModelName;
        if (!string.IsNullOrEmpty(model))
        {
            pawn.SetModel("characters/models/tm_jumpsuit/tm_jumpsuit_varianta.vmdl");
            pawn.SetModel(model);
        }
        Server.NextWorldUpdate(() =>
        {
            if (pawn.IsValid && itemServices.Handle != nint.Zero)
            {
                itemServices.UpdateWearables();
                if (item != null)
                    pawn.AcceptInput("SetBodygroup", value: "default_gloves,1");
            }
        });
    }

    public static void RegiveWeapons(this CCSPlayerController self, PlayerInventory inventory, PlayerInventory? oldInventory)
    {
        var pawn = self.PlayerPawn.Value;
        var weaponServices = pawn?.WeaponServices?.As<CCSPlayer_WeaponServices>();
        if (pawn == null || weaponServices == null)
            return;
        var activeDesignerName = weaponServices.ActiveWeapon.Value?.DesignerName;
        var targets = new List<(string, string, int, int, bool, gear_slot_t)>();
        foreach (var handle in weaponServices.MyWeapons)
        {
            var weapon = handle.Value?.As<CCSWeaponBase>();
            if (weapon == null || weapon.DesignerName.Contains("weapon_") != true)
                continue;
            if (weapon.OriginalOwnerXuidLow != (uint)self.SteamID)
                continue;
            var data = weapon.VData?.As<CCSWeaponBaseVData>();
            if (data != null && data.GearSlot is gear_slot_t.GEAR_SLOT_RIFLE or gear_slot_t.GEAR_SLOT_PISTOL or gear_slot_t.GEAR_SLOT_KNIFE)
            {
                var entityDef = weapon.AttributeManager.Item.ItemDefinitionIndex;
                var isFallbackTeam = ConVars.IsFallbackTeam.Value;
                var oldItem =
                    data.GearSlot is gear_slot_t.GEAR_SLOT_KNIFE
                        ? oldInventory?.GetKnife(self.TeamNum, isFallbackTeam)
                        : oldInventory?.GetWeapon(self.TeamNum, entityDef, isFallbackTeam);
                var item =
                    data.GearSlot is gear_slot_t.GEAR_SLOT_KNIFE
                        ? inventory.GetKnife(self.TeamNum, isFallbackTeam)
                        : inventory.GetWeapon(self.TeamNum, entityDef, isFallbackTeam);
                if (oldItem == item)
                    continue;
                var clip = weapon.Clip1;
                var reserve = weapon.ReserveAmmo[0];
                targets.Add((weapon.DesignerName, weapon.GetDesignerName(), clip, reserve, activeDesignerName == weapon.DesignerName, data.GearSlot));
            }
        }
        foreach (var target in targets)
        {
            var designerName = target.Item1;
            var actualDesignerName = target.Item2;
            var clip = target.Item3;
            var reserve = target.Item4;
            var active = target.Item5;
            var gearSlot = target.Item6;
            var oldWeapon = weaponServices.MyWeapons.FirstOrDefault(h => h.Value?.DesignerName == designerName)?.Value;
            if (oldWeapon != null)
            {
                weaponServices.DropWeapon(oldWeapon);
                oldWeapon.Remove();
            }
            var weapon = self.GiveNamedItem<CBasePlayerWeapon>(actualDesignerName);
            if (weapon != null)
                Server.RunOnTick(
                    Server.TickCount + 32,
                    () =>
                    {
                        if (weapon.IsValid)
                        {
                            weapon.Clip1 = clip;
                            Utilities.SetStateChanged(weapon, "CBasePlayerWeapon", "m_iClip1");
                            weapon.ReserveAmmo[0] = reserve;
                            Utilities.SetStateChanged(weapon, "CBasePlayerWeapon", "m_pReserveAmmo");
                            Server.NextWorldUpdate(() =>
                            {
                                if (active && self.IsValid)
                                {
                                    var command = gearSlot switch
                                    {
                                        gear_slot_t.GEAR_SLOT_RIFLE => "slot1",
                                        gear_slot_t.GEAR_SLOT_PISTOL => "slot2",
                                        gear_slot_t.GEAR_SLOT_KNIFE => "slot3",
                                        _ => null,
                                    };
                                    if (command != null)
                                        self.ExecuteClientCommand(command);
                                }
                            });
                        }
                    }
                );
        }
    }
}
