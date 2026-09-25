/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using Microsoft.Extensions.Logging;

namespace InventorySimulator;

public partial class InventorySimulator
{
    [ConsoleCommand(
        "css_ws",
        "Refreshes player inventory from the Inventory Simulator service and displays the configured URL."
    )]
    public void OnWSCommand(CCSPlayerController? player, CommandInfo _)
    {
        var prefix = Rules.GetChatPrefix();
        var url = UrlHelper.FormatUrl(ConVars.WsUrlPrintFormat.Value, ConVars.Url.Value);
        player?.PrintToChat(Localizer["invsim.announce", prefix, url]);
        if (!ConVars.IsWsEnabled.Value || player == null)
            return;
        var controllerState = player.GetState();
        var cooldown = ConVars.WsCooldown.Value;
        var diff = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - controllerState.WsUpdatedAt;
        if (diff < cooldown)
        {
            player.PrintToChat(Localizer["invsim.ws_cooldown", prefix, cooldown - diff]);
            return;
        }
        if (controllerState.IsFetching)
        {
            player.PrintToChat(Localizer["invsim.ws_in_progress", prefix]);
            return;
        }
        player.RefreshInventory(force: true);
        player.PrintToChat(Localizer["invsim.ws_new", prefix]);
    }

    [ConsoleCommand(
        "css_spray",
        "Applies the player's equipped graffiti spray at their current location."
    )]
    public void OnSprayCommand(CCSPlayerController? player, CommandInfo _)
    {
        if (player != null && ConVars.IsSprayEnabled.Value)
        {
            var controllerState = player.GetState();
            var cooldown = ConVars.SprayCooldown.Value;
            var diff = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - controllerState.SprayUsedAt;
            if (diff < cooldown)
            {
                player.PrintToChat(
                    Localizer["invsim.spray_cooldown", Rules.GetChatPrefix(), cooldown - diff]
                );
                return;
            }
            player.SprayGraffiti();
        }
    }

    [ConsoleCommand(
        "css_invsim_pets",
        "Lists every pet chicken with its owner and look; \"fix\" removes strays right away."
    )]
    [RequiresPermissions("@css/root")]
    public void OnPetsCommand(CCSPlayerController? player, CommandInfo command)
    {
        var removed = command.GetArg(1) == "fix" ? Pets.Reconcile("css_invsim_pets fix") : -1;
        var lines = Pets.Describe();
        if (removed >= 0)
            lines.Insert(0, $"removed {removed}");
        // Hosted consoles may hide command replies; the log always has them.
        foreach (var line in lines)
        {
            command.ReplyToCommand($"[pets] {line}");
            Logger.LogInformation("[pets] {Line}", line);
        }
    }

    [ConsoleCommand(
        "css_wslogin",
        "Authenticates the player with Inventory Simulator and displays their login URL."
    )]
    public void OnWsloginCommand(CCSPlayerController? player, CommandInfo _)
    {
        if (ConVars.IsWsLogin.Value && Api.HasApiKey() && player != null)
        {
            var controllerState = player.GetState();
            player.PrintToChat(Localizer["invsim.login_in_progress", Rules.GetChatPrefix()]);
            if (controllerState.IsAuthenticating)
                return;
            player.SignIn();
        }
    }
}
