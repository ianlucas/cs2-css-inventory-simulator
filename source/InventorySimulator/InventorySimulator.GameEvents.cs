/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core;

namespace InventorySimulator;

public partial class InventorySimulator
{
    public HookResult OnPlayerConnect(EventPlayerConnect @event, GameEventInfo _)
    {
        var player = @event.Userid;
        if (player != null && !player.IsBot)
            HandlePlayerConnect(player);
        return HookResult.Continue;
    }

    public HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo _)
    {
        var player = @event.Userid;
        if (player != null && !player.IsBot)
            HandlePlayerConnect(player);
        return HookResult.Continue;
    }

    public HookResult OnPlayerDeathPre(EventPlayerDeath @event, GameEventInfo _)
    {
        var attacker = @event.Attacker;
        var victim = @event.Userid;
        if (attacker != null && victim != null)
        {
            var isAttackerValid = !attacker.IsBot && attacker.IsValid;
            var isVictimValid =
                (!ConVars.IsStatTrakIgnoreBots.Value || !victim.IsBot) && victim.IsValid;
            if (isAttackerValid && isVictimValid)
                HandlePlayerWeaponStatTrakIncrement(attacker, @event.Weapon, @event.WeaponItemid);
        }
        return HookResult.Continue;
    }

    public HookResult OnRoundMvpPre(EventRoundMvp @event, GameEventInfo _)
    {
        var player = @event.Userid;
        if (player != null && !player.IsBot && player.IsValid)
            HandlePlayerMusicKitStatTrakIncrement(@event, player);
        return HookResult.Continue;
    }
}
