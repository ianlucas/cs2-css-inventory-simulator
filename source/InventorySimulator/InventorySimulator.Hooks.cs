/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;

namespace InventorySimulator;

public partial class InventorySimulator
{
    public HookResult OnActivatePlayerPre(DynamicHook hook)
    {
        var thisPtr = hook.GetParam<nint>(0);
        var userid = new CServerSideClientBase(thisPtr).UserID;
        var player = Utilities.GetPlayerFromUserid(userid);
        if (player != null && !player.IsBot)
        {
            player.Revalidate();
            var controllerState = player.GetState();
            if (controllerState.Inventory == null)
            {
                controllerState.PostFetchCallback = () =>
                    Server.NextWorldUpdate(() =>
                    {
                        if (player.IsValid)
                            Natives.CServerSideClientBase_ActivatePlayer.Invoke(thisPtr);
                    });
                if (!controllerState.IsFetching)
                    player.RefreshInventory();
                return HookResult.Stop;
            }
        }
        return HookResult.Continue;
    }

    public HookResult OnProcessUsercmds(DynamicHook hook)
    {
        if (!ConVars.IsSprayOnUse.Value)
            return HookResult.Continue;
        var player = hook.GetParam<CCSPlayerController>(0);
        player.HandleProcessUsercmds();
        return HookResult.Continue;
    }

    public HookResult OnGiveNamedItemPre(DynamicHook hook)
    {
        var itemServices = hook.GetParam<CCSPlayer_ItemServices>(0);
        var designerName = hook.GetParam<string>(1);
        var controller = itemServices.GetController();
        if (controller?.SteamID != 0 && controller?.InventoryServices != null)
        {
            var itemDef = SchemaHelper.GetItemSchema()?.GetItemDefinitionByName(designerName);
            if (itemDef != null)
            {
                var controllerState = controller.GetState();
                var item = controllerState.Inventory?.GetItemForSlot(
                    controller.TeamNum,
                    itemDef.DefaultLoadoutSlot,
                    itemDef.DefIndex,
                    ConVars.IsFallbackTeam.Value
                );
                if (item != null)
                    hook.SetParam(
                        3,
                        controllerState.GetEconItemView(
                            controller.TeamNum,
                            (int)itemDef.DefaultLoadoutSlot,
                            item
                        )
                    );
            }
        }
        return HookResult.Continue;
    }

    public HookResult GetItemInLoadout(DynamicHook hook)
    {
        var inventory = new CCSPlayerInventory(hook.GetParam<nint>(0));
        if (!inventory.IsValid)
            return HookResult.Continue;
        var slot = hook.GetParam<int>(2);
        var ret = hook.GetReturn<nint>();
        // Unlike other slots, the pet slot has no default item to fall back to.
        if (ret == nint.Zero && slot != (int)loadout_slot_t.LOADOUT_SLOT_PET)
            return HookResult.Continue;
        var player = PlayerHelper.GetPlayerFromSteamId(inventory.SOCache.Owner.SteamID);
        if (player == null)
            return HookResult.Continue;
        var team = hook.GetParam<int>(1);
        var def = ret != nint.Zero ? new CEconItemView(ret).ItemDefinitionIndex : (ushort)0;
        var controllerState = player.GetState();
        var item = controllerState.Inventory?.GetItemForSlot(
            (byte)team,
            (loadout_slot_t)slot,
            def,
            ConVars.IsFallbackTeam.Value,
            ConVars.MinModels.Value
        );
        if (item != null)
        {
            hook.SetReturn(controllerState.GetEconItemView(team, slot, item, ret));
            return HookResult.Changed;
        }
        return HookResult.Continue;
    }
}
