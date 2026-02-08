/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Text.Json.Serialization;

namespace InventorySimulator;

public class EquippedV4Response
{
    [JsonPropertyName("agents")]
    public Dictionary<byte, InventoryItem> Agents { get; set; } = [];

    [JsonPropertyName("collectible")]
    public InventoryItem? Collectible { get; set; }

    [JsonPropertyName("ctWeapons")]
    public Dictionary<ushort, InventoryItem> CTWeapons { get; set; } = [];

    [JsonPropertyName("gloves")]
    public Dictionary<byte, InventoryItem> Gloves { get; set; } = [];

    [JsonPropertyName("graffiti")]
    public InventoryItem? Graffiti { get; set; }

    [JsonPropertyName("knives")]
    public Dictionary<byte, InventoryItem> Knives { get; set; } = [];

    [JsonPropertyName("musicKit")]
    public InventoryItem? MusicKit { get; set; }

    [JsonPropertyName("tWeapons")]
    public Dictionary<ushort, InventoryItem> TWeapons { get; set; } = [];
}
