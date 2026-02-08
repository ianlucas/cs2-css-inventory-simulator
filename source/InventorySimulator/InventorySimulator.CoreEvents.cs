/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace InventorySimulator;

public partial class InventorySimulator
{
    public void OnEntityCreated(CEntityInstance entity)
    {
        var designerName = entity.DesignerName;
        if (designerName == "player_spray_decal")
        {
            if (!ConVars.IsSprayChangerEnabled.Value)
                return;
            Server.NextWorldUpdate(() =>
            {
                var sprayDecal = entity.As<CPlayerSprayDecal>();
                if (!sprayDecal.IsValid || sprayDecal.AccountID == 0)
                    return;
                var player = Utilities.GetPlayerFromSteamId(sprayDecal.AccountID);
                if (player == null || player.IsBot)
                    return;
                HandlePlayerSprayDecalCreated(player, sprayDecal);
            });
        }
    }

    public void OnEntityDeleted(CEntityInstance entity)
    {
        var designerName = entity.DesignerName;
        if (designerName == "cs_player_controller")
        {
            var controller = entity.As<CCSPlayerController>();
            if (controller.SteamID != 0)
                HandleControllerDeleted(controller);
        }
    }
}
