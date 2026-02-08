/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Concurrent;
using CounterStrikeSharp.API.Core;

namespace InventorySimulator;

public static class CCSPlayerControllerExtensions
{
    private static readonly ConcurrentDictionary<uint, CCSPlayerControllerState> _controllerStateManager = [];

    public static CCSPlayerControllerState GetState(this CCSPlayerController self)
    {
        return _controllerStateManager.GetOrAdd(self.Index, _ => new(self.SteamID));
    }

    public static void Revalidate(this CCSPlayerController self)
    {
        if (self.GetState().SteamID != self.SteamID)
            self.RemoveState();
    }

    public static void RemoveState(this CCSPlayerController self)
    {
        var controllerState = self.GetState();
        controllerState.DisposeUseCmdTimer();
        controllerState.ClearEconItemView();
        _controllerStateManager.TryRemove(self.Index, out var _);
    }
}
