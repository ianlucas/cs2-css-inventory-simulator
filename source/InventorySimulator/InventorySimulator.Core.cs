/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace InventorySimulator;

public partial class InventorySimulator
{
    // ========================================================================
    // Connection & Initialization
    // ========================================================================

    public void HandlePlayerConnect(CCSPlayerController player)
    {
        player.Revalidate();
        HandlePlayerInventoryRefresh(player);
    }

    // ========================================================================
    // Inventory Fetch & Load Operations
    // ========================================================================

    public static async Task HandlePlayerInventoryFetch(
        CCSPlayerController player,
        bool force = false
    )
    {
        var controllerState = player.GetState();
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

    public async void HandlePlayerInventoryRefresh(CCSPlayerController player, bool force = false)
    {
        if (!force)
        {
            await HandlePlayerInventoryFetch(player);
            Server.NextWorldUpdate(() =>
            {
                if (player.IsValid)
                    HandlePlayerInventoryLoad(player);
            });
            return;
        }
        var oldInventory = player.GetState().Inventory;
        await HandlePlayerInventoryFetch(player, true);
        Server.NextWorldUpdate(() =>
        {
            if (player.IsValid)
            {
                player.PrintToChat(Localizer["invsim.ws_completed"]);
                HandlePlayerInventoryLoad(player);
                HandlePostPlayerInventoryRefresh(player, oldInventory);
            }
        });
    }

    public static void HandlePlayerInventoryLoad(CCSPlayerController player)
    {
        var inventory = player.InventoryServices?.GetInventory();
        if (inventory?.IsValid == true)
            inventory.SendInventoryUpdateEvent();
    }

    public static void HandlePostPlayerInventoryRefresh(
        CCSPlayerController player,
        PlayerInventory? oldInventory
    )
    {
        var inventory = player.GetState().Inventory;
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
        CCSPlayerController player,
        string designerName,
        string weaponItemId
    )
    {
        var weapon = player.PlayerPawn.Value?.WeaponServices?.ActiveWeapon.Value;
        if (
            weapon == null
            || !weapon.HasCustomItemID()
            || weapon.AttributeManager.Item.AccountID
                != new CSteamID(player.SteamID).GetAccountID().m_AccountID
            || weapon.AttributeManager.Item.ItemID != ulong.Parse(weaponItemId)
        )
            return;
        var inventory = player.GetState().Inventory;
        var isFallbackTeam = ConVars.IsFallbackTeam.Value;
        var item = ItemHelper.IsMeleeDesignerName(designerName)
            ? inventory?.GetKnife(player.TeamNum, isFallbackTeam)
            : inventory?.GetWeapon(
                player.TeamNum,
                weapon.AttributeManager.Item.ItemDefinitionIndex,
                isFallbackTeam
            );
        if (item == null || item.Stattrak == null || item.Uid == null)
            return;
        item.Stattrak += 1;
        var statTrak = TypeHelper.ViewAs<int, float>(item.Stattrak.Value);
        weapon.AttributeManager.Item.NetworkedDynamicAttributes.SetOrAddAttributeValueByName(
            "kill eater",
            statTrak
        );
        HandleStatTrakIncrement(player.SteamID, item.Uid.Value);
    }

    public static void HandlePlayerMusicKitStatTrakIncrement(
        EventRoundMvp @event,
        CCSPlayerController player
    )
    {
        var item = player.GetState().Inventory?.MusicKit;
        if (item != null && item.Uid != null && item.Stattrak != null && item.Stattrak >= 0)
        {
            item.Stattrak += 1;
            @event.Musickitmvps = item.Stattrak.Value;
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

    public void HandleClientProcessUsercmds(CCSPlayerController player)
    {
        if (
            (player.Buttons & PlayerButtons.Use) != 0
            && player.PlayerPawn.Value?.IsAbleToApplySpray() == true
        )
        {
            var controllerState = player.GetState();
            if (player.IsUseCmdBusy())
                controllerState.IsUseCmdBlocked = true;
            controllerState.DisposeUseCmdTimer();
            controllerState.UseCmdTimer = AddTimer(
                0.1f,
                () =>
                {
                    if (controllerState.IsUseCmdBlocked)
                        controllerState.IsUseCmdBlocked = false;
                    else if (player.IsValid && !player.IsUseCmdBusy())
                        player.ExecuteClientCommandFromServer("css_spray");
                }
            );
        }
    }

    public unsafe void HandlePlayerGraffitiSpray(CCSPlayerController player)
    {
        if (!player.IsValid)
            return;
        var item = player.GetState().Inventory?.Graffiti;
        if (item == null || item.Def == null || item.Tint == null)
            return;
        var pawn = player.PlayerPawn.Value;
        if (pawn == null || pawn.LifeState != (int)LifeState_t.LIFE_ALIVE)
            return;
        var movementServices = pawn.MovementServices?.As<CCSPlayer_MovementServices>();
        if (movementServices == null)
            return;
        var trace = stackalloc CGameTrace[1];
        if (!pawn.IsAbleToApplySpray((nint)trace) || (nint)trace == nint.Zero)
            return;
        player.EmitSound("SprayCan.Shake");
        player.GetState().SprayUsedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var endPos = SchemaHelper.ToVector(trace->EndPos);
        var hitNormal = SchemaHelper.ToVector(trace->HitNormal);
        var sprayDecal = Utilities.CreateEntityByName<CPlayerSprayDecal>("player_spray_decal");
        if (sprayDecal != null)
        {
            sprayDecal.EndPos.Add(endPos);
            sprayDecal.Start.Add(endPos);
            sprayDecal.Left.Add(movementServices.Left);
            sprayDecal.Normal.Add(hitNormal);
            sprayDecal.AccountID = (uint)player.SteamID;
            sprayDecal.Player = item.Def.Value;
            sprayDecal.TintID = item.Tint.Value;
            sprayDecal.DispatchSpawn();
            player.EmitSound("SprayCan.Paint");
        }
    }

    public static void HandlePlayerSprayDecalCreated(
        CCSPlayerController player,
        CPlayerSprayDecal sprayDecal
    )
    {
        var item = player.GetState().Inventory?.Graffiti;
        if (item != null && item.Def != null && item.Tint != null)
        {
            sprayDecal.Player = item.Def.Value;
            Utilities.SetStateChanged(sprayDecal, "CPlayerSprayDecal", "m_nPlayer");
            sprayDecal.TintID = item.Tint.Value;
            Utilities.SetStateChanged(sprayDecal, "CPlayerSprayDecal", "m_nTintID");
        }
    }

    // ========================================================================
    // Runtime: Authentication
    // ========================================================================

    public async void HandleSignIn(CCSPlayerController player)
    {
        var controllerState = player.GetState();
        if (controllerState.IsFetching)
            return;
        controllerState.IsFetching = true;
        var response = await Api.SendSignIn(player.SteamID.ToString());
        controllerState.IsFetching = false;
        Server.NextWorldUpdate(() =>
        {
            if (response == null)
            {
                player?.PrintToChat(Localizer["invsim.login_failed"]);
                return;
            }
            player?.PrintToChat(
                Localizer[
                    "invsim.login",
                    $"{Api.GetUrl("/api/sign-in/callback")}?token={response.Token}"
                ]
            );
        });
    }

    // ========================================================================
    // Configuration & File Management
    // ========================================================================

    public void HandleFileChanged(object? _, string value)
    {
        if (Inventories.Load(value))
            foreach (var player in Utilities.GetPlayers().Where(p => !p.IsBot))
                if (Inventories.TryGet(player.SteamID, out var inventory))
                    player.GetState().Inventory = inventory;
    }

    public void HandleIsRequireInventoryChanged(object? _, bool value)
    {
        if (ConVars.IsRequireInventory.Value)
            Natives.CServerSideClientBase_ActivatePlayer.Hook(OnActivatePlayerPre, HookMode.Pre);
        else
            Natives.CServerSideClientBase_ActivatePlayer.Unhook(OnActivatePlayerPre, HookMode.Pre);
    }

    // ========================================================================
    // Cleanup & Disconnection
    // ========================================================================

    public static void HandleControllerDeleted(CCSPlayerController controller)
    {
        controller.RemoveState();
    }
}
