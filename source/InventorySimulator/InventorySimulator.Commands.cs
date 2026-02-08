/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;

namespace InventorySimulator;

public partial class InventorySimulator
{
    [ConsoleCommand("css_ws", "Refreshes player's inventory.")]
    public void OnWSCommand(CCSPlayerController? player, CommandInfo _)
    {
        var url = UrlHelper.FormatUrl(ConVars.WsUrlPrintFormat.Value, ConVars.Url.Value);
        player?.PrintToChat(Localizer["invsim.announce", url]);
        if (!ConVars.IsWsEnabled.Value || player == null)
            return;
        var controllerState = player.GetState();
        var cooldown = ConVars.WsCooldown.Value;
        var diff = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - controllerState.WsUpdatedAt;
        if (diff < cooldown)
        {
            player.PrintToChat(Localizer["invsim.ws_cooldown", cooldown - diff]);
            return;
        }
        if (controllerState.IsFetching)
        {
            player.PrintToChat(Localizer["invsim.ws_in_progress"]);
            return;
        }
        HandlePlayerInventoryRefresh(player, true);
        player.PrintToChat(Localizer["invsim.ws_new"]);
    }

    [ConsoleCommand("css_spray", "Spray player's graffiti.")]
    public void OnSprayCommand(CCSPlayerController? player, CommandInfo _)
    {
        if (player != null && ConVars.IsSprayEnabled.Value)
        {
            var controllerState = player.GetState();
            var cooldown = ConVars.SprayCooldown.Value;
            var diff = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - controllerState.SprayUsedAt;
            if (diff < cooldown)
            {
                player.PrintToChat(Localizer["invsim.spray_cooldown", cooldown - diff]);
                return;
            }
            HandlePlayerGraffitiSpray(player);
        }
    }

    [ConsoleCommand("css_wslogin", "Authenticate player to Inventory Simulator.")]
    public void OnWsloginCommand(CCSPlayerController? player, CommandInfo _)
    {
        if (ConVars.IsWsLogin.Value && Api.HasApiKey() && player != null)
        {
            var controllerState = player.GetState();
            player.PrintToChat(Localizer["invsim.login_in_progress"]);
            if (controllerState.IsAuthenticating)
                return;
            HandleSignIn(player);
        }
    }
}
