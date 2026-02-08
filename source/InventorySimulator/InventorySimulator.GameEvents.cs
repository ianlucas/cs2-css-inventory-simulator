/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Misc;

namespace InventorySimulator;

public partial class InventorySimulator
{
    public HookResult OnPlayerConnect(EventPlayerConnect @event)
    {
        var player = @event.UserIdPlayer;
        if (player != null && !player.IsFakeClient)
            HandlePlayerConnect(player);
        return HookResult.Continue;
    }

    public HookResult OnPlayerConnectFull(EventPlayerConnectFull @event)
    {
        var player = @event.UserIdPlayer;
        if (player != null && !player.IsFakeClient)
            HandlePlayerConnect(player);
        return HookResult.Continue;
    }

    public HookResult OnPlayerDeathPre(EventPlayerDeath @event)
    {
        var attacker = Core.PlayerManager.GetPlayer(@event.Attacker);
        var victim = @event.UserIdPlayer;
        if (attacker != null && victim != null)
        {
            var isAttackerValid = !attacker.IsFakeClient && attacker.IsValid;
            var isVictimValid =
                (!ConVars.IsStatTrakIgnoreBots.Value || !victim.IsFakeClient) && victim.IsValid;
            if (isAttackerValid && isVictimValid)
                HandlePlayerWeaponStatTrakIncrement(attacker, @event.Weapon, @event.WeaponItemid);
        }
        return HookResult.Continue;
    }

    public HookResult OnRoundMvpPre(EventRoundMvp @event)
    {
        var player = @event.UserIdPlayer;
        if (player != null && !player.IsFakeClient && player.IsValid)
            HandlePlayerMusicKitStatTrakIncrement(@event, player);
        return HookResult.Continue;
    }
}
