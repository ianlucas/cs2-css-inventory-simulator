/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using CounterStrikeSharp.API;
using Microsoft.Extensions.Logging;

namespace InventorySimulator;

public static class Inventories
{
    private static readonly Dictionary<ulong, PlayerInventory> _loadedInventories = [];
    private static readonly string _inventoryFileDir = "csgo/addons/swiftlycs2/configs";

    public static bool Load(string filename)
    {
        try
        {
            var path = Path.Combine(Server.GameDirectory, _inventoryFileDir, filename);
            if (!File.Exists(path))
                return false;
            string json = File.ReadAllText(path);
            var inventories = JsonSerializer.Deserialize<Dictionary<ulong, PlayerInventory>>(json);
            _loadedInventories.Clear();
            if (inventories != null)
                foreach (var pair in inventories)
                    _loadedInventories.TryAdd(pair.Key, pair.Value);
            return true;
        }
        catch
        {
            CSS.Plugin.Logger.LogError("Error when processing \"{File}\".", filename);
            return false;
        }
    }

    public static bool TryGet(ulong steamId, [MaybeNullWhen(false)] out PlayerInventory inventory)
    {
        if (_loadedInventories.TryGetValue(steamId, out var value))
        {
            inventory = value;
            return true;
        }
        inventory = default;
        return false;
    }

    public static PlayerInventory? Get(ulong steamId)
    {
        return _loadedInventories.TryGetValue(steamId, out var inventory) ? inventory : null;
    }
}
