/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace InventorySimulator;

public partial class InventorySimulator
{
    public void OnConVarValueChanged(IOnConVarValueChanged @event)
    {
        switch (@event.ConVarName)
        {
            case "invsim_file":
                HandleFileChanged();
                return;
            case "invsim_require_inventory":
                HandleIsRequireInventoryChanged();
                return;
        }
    }

    public void OnEntityCreated(IOnEntityCreatedEvent @event)
    {
        var entity = @event.Entity;
        var designerName = entity.DesignerName;
        if (designerName == "player_spray_decal")
        {
            if (!ConVars.IsSprayChangerEnabled.Value)
                return;
            Core.Scheduler.NextWorldUpdate(() =>
            {
                var sprayDecal = entity.As<CPlayerSprayDecal>();
                if (!sprayDecal.IsValid || sprayDecal.AccountID == 0)
                    return;
                var player = Core.PlayerManager.GetPlayerFromSteamID(sprayDecal.AccountID);
                if (player == null || player.IsFakeClient || !player.IsValid)
                    return;
                HandlePlayerSprayDecalCreated(player, sprayDecal);
            });
        }
    }

    public void OnClientProcessUsercmds(IOnClientProcessUsercmdsEvent @event)
    {
        if (!ConVars.IsSprayOnUse.Value)
            return;
        var player = Core.PlayerManager.GetPlayer(@event.PlayerId);
        if (player != null)
            HandleClientProcessUsercmds(player);
    }

    public void OnEntityDeleted(IOnEntityDeletedEvent @event)
    {
        var entity = @event.Entity;
        var designerName = entity.DesignerName;
        if (designerName == "cs_player_controller")
        {
            var controller = entity.As<CCSPlayerController>();
            if (controller.SteamID != 0)
                HandleControllerDeleted(controller);
        }
    }
}
