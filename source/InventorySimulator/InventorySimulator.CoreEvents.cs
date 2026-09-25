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
                var player = PlayerHelper.GetPlayerFromAccountId(sprayDecal.AccountID);
                if (player == null || player.IsBot)
                    return;
                player.HandleSprayDecalCreated(sprayDecal);
            });
        }
    }

    public void OnMapStart(string _)
    {
        Pets.IsMapUnloading = false;
        CCSPlayerControllerExtensions.ForgetAllPets();
    }

    public void OnMapEnd()
    {
        Pets.IsMapUnloading = true;
        CCSPlayerControllerExtensions.ForgetAllPets();
    }

    public void OnClientDisconnect(int slot)
    {
        Utilities.GetPlayerFromSlot(slot)?.GetState().RemovePet();
    }

    public void OnEntityDeleted(CEntityInstance entity)
    {
        var designerName = entity.DesignerName;
        if (designerName == "chicken")
        {
            var handle = entity.EntityHandle.Raw;
            foreach (var (_, controllerState) in CCSPlayerControllerExtensions.GetStates())
                if (controllerState.PetHandle == handle)
                    controllerState.ForgetPet();
        }
        else if (designerName == "cs_player_controller")
        {
            var controller = entity.As<CCSPlayerController>();
            // A real disconnect has already removed the pet, and a map change deletes it by itself.
            if (controller.SteamID != 0)
                controller.RemoveState(removePet: false);
        }
    }
}
