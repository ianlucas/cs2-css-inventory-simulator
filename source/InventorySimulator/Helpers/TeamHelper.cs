/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Modules.Utils;

namespace InventorySimulator;

public static class TeamHelper
{
    public static byte ToggleTeam(byte team) =>
        team > (byte)CsTeam.Spectator
            ? (byte)((CsTeam)team == CsTeam.Terrorist ? CsTeam.CounterTerrorist : CsTeam.Terrorist)
            : team;
}
