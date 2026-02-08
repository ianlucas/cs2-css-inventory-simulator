/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using SwiftlyS2.Shared.Players;

namespace InventorySimulator;

public static class TeamHelper
{
    public static byte ToggleTeam(byte team) =>
        team > (byte)Team.Spectator ? (byte)((Team)team == Team.T ? Team.CT : Team.T) : team;
}
