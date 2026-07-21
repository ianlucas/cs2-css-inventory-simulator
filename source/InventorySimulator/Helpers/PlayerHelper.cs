/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace InventorySimulator;

public static class PlayerHelper
{
    // Utilities.GetPlayerFromSteamId matches against AuthorizedSteamID, which is only populated
    // after CounterStrikeSharp's 0.5s auth poll observes IsClientFullyAuthenticated, and is reset
    // on every disconnect. It also skips players that aren't in the Connected state. Reconnecting
    // players spawn inside that window, so we match on the controller's networked SteamID instead.
    public static CCSPlayerController? GetPlayerFromSteamId(ulong steamId)
    {
        return GetPlayerFromAccountId(new CSteamID(steamId).GetAccountID().m_AccountID);
    }

    public static CCSPlayerController? GetPlayerFromAccountId(uint accountId)
    {
        if (accountId == 0)
            return null;
        for (var slot = 0; slot < Server.MaxPlayers; slot++)
        {
            var player = Utilities.GetPlayerFromSlot(slot);
            if (
                player?.IsValid == true
                && new CSteamID(player.SteamID).GetAccountID().m_AccountID == accountId
            )
                return player;
        }
        return null;
    }
}
