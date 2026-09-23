/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using CounterStrikeSharp.API.Core;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace InventorySimulator;

public class CCSPlayerControllerState(ulong steamId)
{
    public ulong SteamID = steamId;
    public bool IsFetching = false;
    public bool IsAuthenticating = false;
    public bool IsLoadedFromFile = false;
    public long WsUpdatedAt = 0;
    public long SprayUsedAt = 0;
    public PlayerInventory? Inventory = Inventories.Get(steamId);
    public Timer? UseCmdTimer;
    public bool IsUseCmdBlocked = false;
    public Action? PostFetchCallback;

    private static readonly ConcurrentDictionary<
        (ulong SteamID, int Team, int Slot),
        (nint Ptr, InventoryItem Item, int? Stattrak)
    > _econItemViewManager = [];

    public void TriggerPostFetch()
    {
        if (PostFetchCallback != null)
        {
            PostFetchCallback();
            PostFetchCallback = null;
        }
    }

    public void DisposeUseCmdTimer()
    {
        UseCmdTimer?.Kill();
        UseCmdTimer = null;
    }

    public nint GetEconItemView(int team, int slot, InventoryItem item, nint copyFrom = 0)
    {
        var key = (SteamID, team, slot);
        if (_econItemViewManager.TryGetValue(key, out var entry))
        {
            if (!ReferenceEquals(entry.Item, item) || entry.Stattrak != item.Stattrak)
            {
                var existingItemView = new CEconItemView(entry.Ptr);
                existingItemView.ApplyAttributes(item, (loadout_slot_t)slot, SteamID);
                _econItemViewManager[key] = (entry.Ptr, item, item.Stattrak);
            }
            return entry.Ptr;
        }
        var itemView = SchemaHelper.CreateCEconItemView(copyFrom);
        itemView.ApplyAttributes(item, (loadout_slot_t)slot, SteamID);
        _econItemViewManager[key] = (itemView.Handle, item, item.Stattrak);
        return itemView.Handle;
    }

    public void ClearEconItemView()
    {
        foreach (var key in _econItemViewManager.Keys)
            if (key.SteamID == SteamID)
                if (_econItemViewManager.TryRemove(key, out var entry))
                    Marshal.FreeHGlobal(entry.Ptr);
    }

    public static void ClearAllEconItemView()
    {
        foreach (var entry in _econItemViewManager.Values)
            Marshal.FreeHGlobal(entry.Ptr);
    }
}
