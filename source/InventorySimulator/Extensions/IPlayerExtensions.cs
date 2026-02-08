/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace InventorySimulator;

public static class IPlayerExtensions
{
    extension(IPlayer self)
    {
        public bool IsUseCmdBusy()
        {
            if (self.PlayerPawn?.IsBuyMenuOpen == true)
                return true;
            if (self.PlayerPawn?.IsDefusing == true)
                return true;
            var weapon = self.PlayerPawn?.WeaponServices?.ActiveWeapon.Value;
            if (weapon?.DesignerName != "weapon_c4")
                return false;
            var c4 = weapon.As<CC4>();
            return c4.IsPlantingViaUse;
        }

        public void RegiveAgent(PlayerInventory inventory, PlayerInventory? oldInventory)
        {
            if (ConVars.MinModels.Value > 0)
                return;
            var pawn = self.PlayerPawn;
            if (pawn == null)
                return;
            var teamNum = self.Controller.TeamNum;
            var item = inventory.Agents.TryGetValue(teamNum, out var a) ? a : null;
            var oldItem =
                oldInventory != null && oldInventory.Agents.TryGetValue(teamNum, out a) ? a : null;
            if (oldItem == item)
                return;
            pawn.SetModelFromLoadout();
            pawn.SetModelFromClass();
            pawn.AcceptInput("SetBodygroup", "default_gloves,1");
        }

        public void RegiveGloves(PlayerInventory inventory, PlayerInventory? oldInventory)
        {
            var pawn = self.PlayerPawn;
            var itemServices = pawn?.ItemServices;
            if (pawn == null || itemServices == null)
                return;
            var isFallbackTeam = ConVars.IsFallbackTeam.Value;
            var teamNum = self.Controller.TeamNum;
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
            Swiftly.Core.Scheduler.NextWorldUpdate(() =>
            {
                if (pawn.IsValid && itemServices.IsValid)
                {
                    itemServices.UpdateWearables();
                    if (item != null)
                        pawn.AcceptInput("SetBodygroup", "default_gloves,1");
                }
            });
        }

        public void RegiveWeapons(PlayerInventory inventory, PlayerInventory? oldInventory)
        {
            var pawn = self.PlayerPawn;
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
                var data = weapon.VData.As<CCSWeaponBaseVData>();
                if (
                    data.GearSlot
                    is gear_slot_t.GEAR_SLOT_RIFLE
                        or gear_slot_t.GEAR_SLOT_PISTOL
                        or gear_slot_t.GEAR_SLOT_KNIFE
                )
                {
                    var entityDef = weapon.AttributeManager.Item.ItemDefinitionIndex;
                    var isFallbackTeam = ConVars.IsFallbackTeam.Value;
                    var oldItem =
                        data.GearSlot is gear_slot_t.GEAR_SLOT_KNIFE
                            ? oldInventory?.GetKnife(self.Controller.TeamNum, isFallbackTeam)
                            : oldInventory?.GetWeapon(
                                self.Controller.TeamNum,
                                entityDef,
                                isFallbackTeam
                            );
                    var item =
                        data.GearSlot is gear_slot_t.GEAR_SLOT_KNIFE
                            ? inventory.GetKnife(self.Controller.TeamNum, isFallbackTeam)
                            : inventory.GetWeapon(
                                self.Controller.TeamNum,
                                entityDef,
                                isFallbackTeam
                            );
                    if (oldItem == item)
                        continue;
                    var clip = weapon.Clip1;
                    var reserve = weapon.ReserveAmmo[0];
                    targets.Add(
                        (
                            weapon.DesignerName,
                            weapon.GetDesignerName(),
                            clip,
                            reserve,
                            activeDesignerName == weapon.DesignerName,
                            data.GearSlot
                        )
                    );
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
                var oldWeapon = (
                    (CHandle<CBasePlayerWeapon>?)
                        weaponServices.MyWeapons.FirstOrDefault(h =>
                            h.Value?.DesignerName == designerName
                        )
                )?.Value;
                if (oldWeapon != null)
                {
                    weaponServices.DropWeapon(oldWeapon);
                    oldWeapon.Despawn();
                }
                var weapon = self.PlayerPawn?.ItemServices?.GiveItem<CBasePlayerWeapon>(
                    actualDesignerName
                );
                if (weapon != null)
                    Swiftly.Core.Scheduler.Delay(
                        32,
                        () =>
                        {
                            if (weapon.IsValid)
                            {
                                weapon.Clip1 = clip;
                                weapon.Clip1Updated();
                                weapon.ReserveAmmo[0] = reserve;
                                weapon.ReserveAmmoUpdated();
                                Swiftly.Core.Scheduler.NextWorldUpdate(() =>
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
                                            self.ExecuteCommand(command);
                                    }
                                });
                            }
                        }
                    );
            }
        }
    }
}
