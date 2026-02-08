/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Text.Json.Serialization;

namespace InventorySimulator;

public class InventoryItem
{
    [JsonPropertyName("def")]
    public ushort? Def { get; set; }

    [JsonPropertyName("hash")]
    public string? Hash { get; set; }

    [JsonPropertyName("musicId")]
    public int? MusicId { get; set; }

    [JsonPropertyName("nametag")]
    public string? Nametag { get; set; }

    [JsonPropertyName("paint")]
    public int? Paint { get; set; }

    [JsonPropertyName("seed")]
    public int? Seed { get; set; }

    [JsonPropertyName("stattrak")]
    public int? Stattrak { get; set; }

    [JsonPropertyName("stickers")]
    public List<StickerItem>? Stickers { get; set; }

    [JsonPropertyName("tint")]
    public int? Tint { get; set; }

    [JsonPropertyName("uid")]
    public int? Uid { get; set; }

    [JsonPropertyName("wear")]
    public float? Wear { get; set; }

    public float? WearOverride { get; set; }

    private (int? statTrak, List<(string, float)> attributes)? _attributesCache;

    public override int GetHashCode()
    {
        return Hash?.GetHashCode() ?? 0;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not InventoryItem other)
            return false;
        return Hash == other.Hash;
    }

    public static bool operator ==(InventoryItem? left, InventoryItem? right)
    {
        if (ReferenceEquals(left, right))
            return true;
        if (left is null || right is null)
            return false;
        return left.Equals(right);
    }

    public static bool operator !=(InventoryItem? left, InventoryItem? right)
    {
        return !(left == right);
    }

    public List<(string, float)> GetAttributes()
    {
        if (_attributesCache != null && _attributesCache.Value.statTrak == Stattrak)
            return _attributesCache.Value.attributes;
        var attributes = new List<(string, float)>();
        if (Paint != null)
            attributes.Add(("set item texture prefab", Paint.Value));
        if (Seed != null)
            attributes.Add(("set item texture seed", Seed.Value));
        var wear = WearOverride ?? Wear;
        if (wear != null)
            attributes.Add(("set item texture wear", wear.Value));
        if (Stattrak != null && Stattrak > -1)
        {
            var statTrak = TypeHelper.ViewAs<int, float>(Stattrak.Value);
            attributes.Add(("kill eater", statTrak));
            attributes.Add(("kill eater score type", 0));
        }
        if (Stickers != null)
            foreach (var sticker in Stickers)
            {
                var slot = $"sticker slot {sticker.Slot}";
                var id = TypeHelper.ViewAs<uint, float>(sticker.Def);
                attributes.Add(($"{slot} id", id));
                if (sticker.Wear != null)
                    attributes.Add(($"{slot} wear", sticker.Wear.Value));
                if (sticker.Rotation != null)
                    attributes.Add(($"{slot} rotation", sticker.Rotation.Value));
                if (sticker.X != null)
                    attributes.Add(($"{slot} offset x", sticker.X.Value));
                if (sticker.Y != null)
                    attributes.Add(($"{slot} offset y", sticker.Y.Value));
            }
        if (MusicId != null)
        {
            var musicId = TypeHelper.ViewAs<int, float>(MusicId.Value);
            attributes.Add(("music id", musicId));
        }
        _attributesCache = (Stattrak, attributes);
        return attributes;
    }
}
