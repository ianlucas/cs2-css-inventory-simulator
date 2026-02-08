/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Text.Json.Serialization;

namespace InventorySimulator;

public class StickerItem
{
    [JsonPropertyName("def")]
    public uint Def { get; set; }

    [JsonPropertyName("rotation")]
    public int? Rotation { get; set; }

    [JsonPropertyName("slot")]
    public uint Slot { get; set; }

    [JsonPropertyName("wear")]
    public float? Wear { get; set; }

    [JsonPropertyName("x")]
    public float? X { get; set; }

    [JsonPropertyName("y")]
    public float? Y { get; set; }
}
