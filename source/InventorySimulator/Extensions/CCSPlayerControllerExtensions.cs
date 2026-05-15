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
    private static readonly ConcurrentDictionary<
        uint,
        CCSPlayerControllerState
    > _controllerStateManager = [];

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

    public static void HandleConnect(this CCSPlayerController self)
    {
        self.Revalidate();
        self.RefreshInventory();
    }

    public static async void RefreshInventory(this CCSPlayerController self, bool force = false)
    {
        if (!force)
        {
            await self.FetchInventory();
            Server.NextWorldUpdate(() =>
            {
                if (self.IsValid)
                    self.HandleInventoryLoad();
            });
            return;
        }
        var oldInventory = self.GetState().Inventory;
        await self.FetchInventory(force: true);
        Server.NextWorldUpdate(() =>
        {
            if (self.IsValid)
            {
                self.PrintToChat(
                    CSS.Plugin.Localizer[
                        "invsim.ws_completed",
                        InventorySimulatorCtx.GetChatPrefix()
                    ]
                );
                self.HandleInventoryLoad();
                self.HandlePostRefreshInventory(oldInventory);
            }
        });
    }

    public static async Task FetchInventory(this CCSPlayerController self, bool force = false)
    {
        var controllerState = self.GetState();
        var existing = controllerState.Inventory;
        if (!force && controllerState.Inventory != null)
            return;
        if (controllerState.IsFetching)
            return;
        controllerState.IsFetching = true;
        var response = await Api.FetchEquippedAsync(self.SteamID);
        if (response != null)
        {
            var inventory = new PlayerInventory(response);
            if (existing != null)
                inventory.WeaponWearCache = existing.WeaponWearCache;
            inventory.InitializeWearOverrides();
            controllerState.WsUpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            controllerState.Inventory = inventory;
        }
        controllerState.IsFetching = false;
        controllerState.TriggerPostFetch();
    }

    public static void HandleInventoryLoad(this CCSPlayerController self)
    {
        var inventory = self.InventoryServices?.GetInventory();
        if (inventory?.IsValid == true)
            inventory.SendInventoryUpdateEvent();
    }

    public static void HandlePostRefreshInventory(
        this CCSPlayerController self,
        PlayerInventory? oldInventory
    )
    {
        var inventory = self.GetState().Inventory;
        if (inventory != null && ConVars.IsWsImmediately.Value)
        {
            self.RegiveAgent(inventory, oldInventory);
            self.RegiveGloves(inventory, oldInventory);
            self.RegiveWeapons(inventory, oldInventory);
        }
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

    public static void HandleProcessUsercmds(this CCSPlayerController self)
    {
        if (
            (self.Buttons & PlayerButtons.Use) != 0
            && self.PlayerPawn.Value?.IsAbleToApplySpray() == true
        )
        {
            var controllerState = self.GetState();
            if (self.IsUseCmdBusy())
                controllerState.IsUseCmdBlocked = true;
            controllerState.DisposeUseCmdTimer();
            controllerState.UseCmdTimer = CSS.Plugin.AddTimer(
                0.1f,
                () =>
                {
                    if (controllerState.IsUseCmdBlocked)
                        controllerState.IsUseCmdBlocked = false;
                    else if (self.IsValid && !self.IsUseCmdBusy())
                        self.ExecuteClientCommandFromServer("css_spray");
                }
            );
        }
    }

    public static void RegiveAgent(
        this CCSPlayerController self,
        PlayerInventory inventory,
        PlayerInventory? oldInventory
    )
    {
        if (ConVars.MinModels.Value > 0)
            return;
        var pawn = self.PlayerPawn.Value;
        if (pawn == null)
            return;
        var teamNum = self.TeamNum;
        var item = inventory.Agents.TryGetValue(teamNum, out var a) ? a : null;
        var oldItem =
            oldInventory != null && oldInventory.Agents.TryGetValue(teamNum, out a) ? a : null;
        if (oldItem == item)
            return;
        pawn.SetModelFromLoadout();
        pawn.SetModelFromClass();
        pawn.AcceptInput("SetBodygroup", value: "default_gloves,1");
    }

    public static void RegiveGloves(
        this CCSPlayerController self,
        PlayerInventory inventory,
        PlayerInventory? oldInventory
    )
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
        itemServices.UpdateWearables();
        // Thanks to @samyycX.
        pawn.AcceptInput("SetBodygroup", value: "first_or_third_person,0");
        Server.NextWorldUpdate(() =>
        {
            if (pawn.IsValid && itemServices.Handle != nint.Zero)
                pawn.AcceptInput("SetBodygroup", value: "first_or_third_person,1");
        });
    }

    public static void RegiveWeapons(
        this CCSPlayerController self,
        PlayerInventory inventory,
        PlayerInventory? oldInventory
    )
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
            if (
                data != null
                && data.GearSlot
                    is gear_slot_t.GEAR_SLOT_RIFLE
                        or gear_slot_t.GEAR_SLOT_PISTOL
                        or gear_slot_t.GEAR_SLOT_KNIFE
            )
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
            var oldWeapon = weaponServices
                .MyWeapons.FirstOrDefault(h => h.Value?.DesignerName == designerName)
                ?.Value;
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
                            Utilities.SetStateChanged(
                                weapon,
                                "CBasePlayerWeapon",
                                "m_pReserveAmmo"
                            );
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

    public static async void SignIn(this CCSPlayerController self)
    {
        var controllerState = self.GetState();
        if (controllerState.IsFetching)
            return;
        controllerState.IsFetching = true;
        var response = await Api.SendSignIn(self.SteamID.ToString());
        controllerState.IsFetching = false;
        Server.NextWorldUpdate(() =>
        {
            var prefix = InventorySimulatorCtx.GetChatPrefix();
            if (response == null)
            {
                self?.PrintToChat(CSS.Plugin.Localizer["invsim.login_failed", prefix]);
                return;
            }
            self?.PrintToChat(
                CSS.Plugin.Localizer[
                    "invsim.login",
                    prefix,
                    $"{Api.GetUrl("/api/sign-in/callback")}?token={response.Token}"
                ]
            );
        });
    }

    public static unsafe void SprayGraffiti(this CCSPlayerController self)
    {
        if (!self.IsValid)
            return;
        var item = self.GetState().Inventory?.Graffiti;
        if (item == null || item.Def == null || item.Tint == null)
            return;
        var pawn = self.PlayerPawn.Value;
        if (pawn == null || pawn.LifeState != (int)LifeState_t.LIFE_ALIVE)
            return;
        var movementServices = pawn.MovementServices?.As<CCSPlayer_MovementServices>();
        if (movementServices == null)
            return;
        var trace = stackalloc CGameTrace[1];
        if (!pawn.IsAbleToApplySpray((nint)trace) || (nint)trace == nint.Zero)
            return;
        self.EmitSound("SprayCan.Shake");
        self.GetState().SprayUsedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var endPos = SchemaHelper.ToVector(trace->EndPos);
        var hitNormal = SchemaHelper.ToVector(trace->HitNormal);
        var sprayDecal = Utilities.CreateEntityByName<CPlayerSprayDecal>("player_spray_decal");
        if (sprayDecal != null)
        {
            sprayDecal.EndPos.Add(endPos);
            sprayDecal.Start.Add(endPos);
            sprayDecal.Left.Add(movementServices.Left);
            sprayDecal.Normal.Add(hitNormal);
            sprayDecal.AccountID = (uint)self.SteamID;
            sprayDecal.Player = item.Def.Value;
            sprayDecal.TintID = item.Tint.Value;
            sprayDecal.DispatchSpawn();
            self.EmitSound("SprayCan.Paint");
        }
    }

    public static void HandleSprayDecalCreated(
        this CCSPlayerController self,
        CPlayerSprayDecal sprayDecal
    )
    {
        var item = self.GetState().Inventory?.Graffiti;
        if (item != null && item.Def != null && item.Tint != null)
        {
            sprayDecal.Player = item.Def.Value;
            Utilities.SetStateChanged(sprayDecal, "CPlayerSprayDecal", "m_nPlayer");
            sprayDecal.TintID = item.Tint.Value;
            Utilities.SetStateChanged(sprayDecal, "CPlayerSprayDecal", "m_nTintID");
        }
    }

    public static void IncrementWeaponStatTrak(
        this CCSPlayerController self,
        string designerName,
        string weaponItemId
    )
    {
        var weapon = self.PlayerPawn.Value?.WeaponServices?.ActiveWeapon.Value;
        if (
            weapon == null
            || !weapon.HasCustomItemID()
            || !ulong.TryParse(weaponItemId, out var parsedItemId)
            || weapon.AttributeManager.Item.AccountID
                != new CSteamID(self.SteamID).GetAccountID().m_AccountID
            || weapon.AttributeManager.Item.ItemID != parsedItemId
        )
            return;
        var inventory = self.GetState().Inventory;
        var isFallbackTeam = ConVars.IsFallbackTeam.Value;
        var item = ItemHelper.IsMeleeDesignerName(designerName)
            ? inventory?.GetKnife(self.TeamNum, isFallbackTeam)
            : inventory?.GetWeapon(
                self.TeamNum,
                weapon.AttributeManager.Item.ItemDefinitionIndex,
                isFallbackTeam
            );
        if (item == null || item.Stattrak == null || item.Stattrak < 0 || item.Uid == null)
            return;
        item.Stattrak += 1;
        var statTrak = TypeHelper.ViewAs<int, float>(item.Stattrak.Value);
        weapon.AttributeManager.Item.NetworkedDynamicAttributes.SetOrAddAttributeValueByName(
            "kill eater",
            statTrak
        );
        Utilities.SetStateChanged(weapon, "CBasePlayerWeapon", "m_AttributeManager");
        Api.SendStatTrakIncrement(self.SteamID, item.Uid.Value);
    }

    public static void IncrementMusicKitStatTrak(
        this CCSPlayerController self,
        EventRoundMvp @event
    )
    {
        var item = self.GetState().Inventory?.MusicKit;
        if (item != null && item.Uid != null && item.Stattrak != null && item.Stattrak >= 0)
        {
            item.Stattrak += 1;
            @event.Musickitmvps = item.Stattrak.Value;
            Api.SendStatTrakIncrement(self.SteamID, item.Uid.Value);
        }
    }

    public static void HandleDisconnect(this CCSPlayerController self)
    {
        if (!ConVars.IsPersistInventory.Value && !Inventories.Has(self.SteamID))
            self.GetState().Inventory = null;
    }
}
