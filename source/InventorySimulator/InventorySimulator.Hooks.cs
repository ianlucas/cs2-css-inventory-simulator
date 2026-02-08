/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Runtime.InteropServices;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace InventorySimulator;

public partial class InventorySimulator
{
    public Natives.CServerSideClientBase_ActivatePlayerDelegate OnActivatePlayer(
        Func<Natives.CServerSideClientBase_ActivatePlayerDelegate> next
    )
    {
        return (thisPtr) =>
        {
            var userid = (ushort)
                Marshal.ReadInt16(thisPtr + Natives.CServerSideClientBase_m_UserID);
            var player = Core.PlayerManager.GetPlayer(userid);
            if (player != null && !player.IsFakeClient && player.Controller != null)
            {
                player.Controller.Revalidate();
                var controllerState = player.Controller.GetState();
                if (controllerState.Inventory == null)
                {
                    controllerState.PostFetchCallback = () =>
                        Core.Scheduler.NextWorldUpdate(() =>
                        {
                            if (player.Controller.IsValid)
                                Natives.CServerSideClientBase_ActivatePlayer.CallOriginal(thisPtr);
                        });
                    if (!controllerState.IsFetching)
                        HandlePlayerInventoryRefresh(player);
                    return;
                }
            }
            next()(thisPtr);
        };
    }

    public Natives.CCSPlayer_ItemServices_GiveNamedItemDelegate OnGiveNamedItem(
        Func<Natives.CCSPlayer_ItemServices_GiveNamedItemDelegate> next
    )
    {
        return (thisPtr, pchName, a3, pScriptItem, a5, a6) =>
        {
            var designerName = Marshal.PtrToStringUTF8(pchName);
            if (designerName != null && pScriptItem == nint.Zero)
            {
                var itemServices = Core.Memory.ToSchemaClass<CCSPlayer_ItemServices>(thisPtr);
                var controller = itemServices.GetController();
                if (controller?.SteamID != 0 && controller?.InventoryServices != null)
                {
                    var itemDef = SchemaHelper
                        .GetItemSchema()
                        ?.GetItemDefinitionByName(designerName);
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
                            pScriptItem = controllerState.GetEconItemView(
                                controller.TeamNum,
                                (int)itemDef.DefaultLoadoutSlot,
                                item
                            );
                    }
                }
            }
            return next()(thisPtr, pchName, a3, pScriptItem, a5, a6);
        };
    }

    public Natives.CCSPlayerInventory_GetItemInLoadoutDelegate OnGetItemInLoadout(
        Func<Natives.CCSPlayerInventory_GetItemInLoadoutDelegate> next
    )
    {
        return (thisPtr, team, slot) =>
        {
            var ret = next()(thisPtr, team, slot);
            var inventory = new CCSPlayerInventory(thisPtr);
            if (!inventory.IsValid)
                return ret;
            var itemView = Core.Memory.ToSchemaClass<CEconItemView>(ret);
            if (!itemView.IsValid)
                return ret;
            var player = Core.PlayerManager.GetPlayerFromSteamID(inventory.SOCache.Owner.SteamID);
            if (player == null)
                return ret;
            var controllerState = player.Controller.GetState();
            var item = controllerState.Inventory?.GetItemForSlot(
                (byte)team,
                (loadout_slot_t)slot,
                itemView.ItemDefinitionIndex,
                ConVars.IsFallbackTeam.Value,
                ConVars.MinModels.Value
            );
            if (item != null)
                return controllerState.GetEconItemView(team, slot, item, ret);
            return ret;
        };
    }
}
