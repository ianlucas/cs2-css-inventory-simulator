/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;
using SwiftlyS2.Shared.SteamAPI;

namespace InventorySimulator;

public partial class InventorySimulator
{
    // ========================================================================
    // Connection & Initialization
    // ========================================================================

    public void HandlePlayerConnect(IPlayer player)
    {
        player.Controller.Revalidate();
        HandlePlayerInventoryRefresh(player);
    }

    // ========================================================================
    // Inventory Fetch & Load Operations
    // ========================================================================

    public static async Task HandlePlayerInventoryFetch(IPlayer player, bool force = false)
    {
        var controllerState = player.Controller.GetState();
        var existing = controllerState.Inventory;
        if (!force && controllerState.Inventory != null)
            return;
        if (controllerState.IsFetching)
            return;
        controllerState.IsFetching = true;
        var response = await Api.FetchEquipped(player.SteamID);
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

    public async void HandlePlayerInventoryRefresh(IPlayer player, bool force = false)
    {
        if (!force)
        {
            await HandlePlayerInventoryFetch(player);
            Core.Scheduler.NextWorldUpdate(() =>
            {
                if (player.IsValid)
                    HandlePlayerInventoryLoad(player);
            });
            return;
        }
        var oldInventory = player.Controller.GetState().Inventory;
        await HandlePlayerInventoryFetch(player, true);
        Core.Scheduler.NextWorldUpdate(() =>
        {
            if (player.IsValid)
            {
                player.SendChat(Core.Localizer["invsim.ws_completed"]);
                HandlePlayerInventoryLoad(player);
                HandlePostPlayerInventoryRefresh(player, oldInventory);
            }
        });
    }

    public static void HandlePlayerInventoryLoad(IPlayer player)
    {
        var inventory = player.Controller.InventoryServices?.GetInventory();
        if (inventory?.IsValid == true)
            inventory.SendInventoryUpdateEvent();
    }

    public static void HandlePostPlayerInventoryRefresh(
        IPlayer player,
        PlayerInventory? oldInventory
    )
    {
        var inventory = player.Controller.GetState().Inventory;
        if (inventory != null && ConVars.IsWsImmediately.Value)
        {
            player.RegiveAgent(inventory, oldInventory);
            player.RegiveGloves(inventory, oldInventory);
            player.RegiveWeapons(inventory, oldInventory);
        }
    }

    // ========================================================================
    // Runtime: StatTrak Operations
    // ========================================================================

    public void HandlePlayerWeaponStatTrakIncrement(
        IPlayer player,
        string designerName,
        string weaponItemId
    )
    {
        var weapon = player.PlayerPawn?.WeaponServices?.ActiveWeapon.Value;
        if (
            weapon == null
            || !weapon.HasCustomItemID()
            || weapon.AttributeManager.Item.AccountID
                != new CSteamID(player.SteamID).GetAccountID().m_AccountID
            || weapon.AttributeManager.Item.ItemID != ulong.Parse(weaponItemId)
        )
            return;
        var inventory = player.Controller.GetState().Inventory;
        var isFallbackTeam = ConVars.IsFallbackTeam.Value;
        var item = ItemHelper.IsMeleeDesignerName(designerName)
            ? inventory?.GetKnife(player.Controller.TeamNum, isFallbackTeam)
            : inventory?.GetWeapon(
                player.Controller.TeamNum,
                weapon.AttributeManager.Item.ItemDefinitionIndex,
                isFallbackTeam
            );
        if (item == null || item.Stattrak == null || item.Uid == null)
            return;
        item.Stattrak += 1;
        var statTrak = TypeHelper.ViewAs<int, float>(item.Stattrak.Value);
        weapon.AttributeManager.Item.NetworkedDynamicAttributes.SetOrAddAttribute(
            "kill eater",
            statTrak
        );
        HandleStatTrakIncrement(player.SteamID, item.Uid.Value);
    }

    public static void HandlePlayerMusicKitStatTrakIncrement(EventRoundMvp @event, IPlayer player)
    {
        var item = player.Controller.GetState().Inventory?.MusicKit;
        if (item != null && item.Uid != null && item.Stattrak != null && item.Stattrak >= 0)
        {
            item.Stattrak += 1;
            @event.MusickItMvps = item.Stattrak.Value;
            HandleStatTrakIncrement(player.SteamID, item.Uid.Value);
        }
    }

    public static async void HandleStatTrakIncrement(ulong userId, int targetUid)
    {
        if (Api.HasApiKey())
            await Api.SendStatTrakIncrement(targetUid, userId.ToString());
    }

    // ========================================================================
    // Runtime: Graffiti/Spray Operations
    // ========================================================================

    public void HandleClientProcessUsercmds(IPlayer player)
    {
        if (
            (player.PressedButtons & GameButtonFlags.E) != 0
            && player.PlayerPawn?.IsAbleToApplySpray() == true
        )
        {
            var controllerState = player.Controller.GetState();
            if (player.IsUseCmdBusy())
                controllerState.IsUseCmdBlocked = true;
            controllerState.DisposeUseCmdTimer();
            controllerState.UseCmdTimer = Core.Scheduler.DelayBySeconds(
                0.1f,
                () =>
                {
                    if (controllerState.IsUseCmdBlocked)
                        controllerState.IsUseCmdBlocked = false;
                    else if (player.IsValid && !player.IsUseCmdBusy())
                        player.ExecuteCommand("css_spray");
                }
            );
        }
    }

    public unsafe void HandlePlayerGraffitiSpray(IPlayer player)
    {
        if (!player.IsValid)
            return;
        var item = player.Controller.GetState().Inventory?.Graffiti;
        if (item == null || item.Def == null || item.Tint == null)
            return;
        var pawn = player.PlayerPawn;
        if (pawn == null || pawn.LifeState != (int)LifeState_t.LIFE_ALIVE)
            return;
        var movementServices = pawn.MovementServices?.As<CCSPlayer_MovementServices>();
        if (movementServices == null)
            return;
        var trace = stackalloc CGameTrace[1];
        if (!pawn.IsAbleToApplySpray((nint)trace) || (nint)trace == nint.Zero)
            return;
        SprayCanShakeSound.Recipients.AddRecipient(player.PlayerID);
        SprayCanShakeSound.Emit();
        SprayCanShakeSound.Recipients.RemoveRecipient(player.PlayerID);
        player.Controller.GetState().SprayUsedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var sprayDecal = Core.EntitySystem.CreateEntityByDesignerName<CPlayerSprayDecal>(
            "player_spray_decal"
        );
        if (sprayDecal != null)
        {
            sprayDecal.EndPos += trace->EndPos;
            sprayDecal.Start += trace->EndPos;
            sprayDecal.Left += movementServices.Left;
            sprayDecal.Normal += trace->HitNormal;
            sprayDecal.AccountID = (uint)player.SteamID;
            sprayDecal.Player = item.Def.Value;
            sprayDecal.TintID = item.Tint.Value;
            sprayDecal.DispatchSpawn();
            SprayCanPaintSound.Recipients.AddRecipient(player.PlayerID);
            SprayCanPaintSound.Emit();
            SprayCanPaintSound.Recipients.RemoveRecipient(player.PlayerID);
        }
    }

    public static void HandlePlayerSprayDecalCreated(IPlayer player, CPlayerSprayDecal sprayDecal)
    {
        var item = player.Controller.GetState().Inventory?.Graffiti;
        if (item != null && item.Def != null && item.Tint != null)
        {
            sprayDecal.Player = item.Def.Value;
            sprayDecal.PlayerUpdated();
            sprayDecal.TintID = item.Tint.Value;
            sprayDecal.TintIDUpdated();
        }
    }

    // ========================================================================
    // Runtime: Authentication
    // ========================================================================

    public async void HandleSignIn(IPlayer player)
    {
        var controllerState = player.Controller.GetState();
        if (controllerState.IsFetching)
            return;
        controllerState.IsFetching = true;
        var response = await Api.SendSignIn(player.SteamID.ToString());
        controllerState.IsFetching = false;
        Core.Scheduler.NextWorldUpdate(() =>
        {
            if (response == null)
            {
                player?.SendChat(Core.Localizer["invsim.login_failed"]);
                return;
            }
            player?.SendChat(
                Core.Localizer[
                    "invsim.login",
                    $"{Api.GetUrl("/api/sign-in/callback")}?token={response.Token}"
                ]
            );
        });
    }

    // ========================================================================
    // Configuration & File Management
    // ========================================================================

    public void HandleFileChanged()
    {
        if (Inventories.Load())
            foreach (var player in Core.PlayerManager.GetAllPlayers())
                if (Inventories.TryGet(player.SteamID, out var inventory))
                    player.Controller.GetState().Inventory = inventory;
    }

    public void HandleIsRequireInventoryChanged()
    {
        if (ConVars.IsRequireInventory.Value)
            OnActivatePlayerHookGuid = Natives.CServerSideClientBase_ActivatePlayer.AddHook(
                OnActivatePlayer
            );
        else if (OnActivatePlayerHookGuid != null)
            Natives.CServerSideClientBase_ActivatePlayer.RemoveHook(OnActivatePlayerHookGuid.Value);
    }

    // ========================================================================
    // Cleanup & Disconnection
    // ========================================================================

    public static void HandleControllerDeleted(CCSPlayerController controller)
    {
        controller.RemoveState();
    }
}
