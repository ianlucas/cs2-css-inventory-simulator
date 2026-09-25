/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace InventorySimulator;

public static class PetHelper
{
    public const string TargetNamePrefix = "invsim_pet_";

    // A breed is its own model; a coat is one of that model's material groups, named "1".."N" in
    // the order the game's inventory lists them. The chick model has a single look.
    private static readonly Dictionary<int, (string Model, int Coats)> _breeds = new()
    {
        [2] = ("models/chicken/chick.vmdl", 0),
        [3] = ("models/chicken/chicken.vmdl", 13),
        [4] = ("models/chicken/chicken_silkie.vmdl", 9),
        [5] = ("models/chicken/chicken_polish.vmdl", 12),
    };

    public static bool TryGetModel(int petId, out string model)
    {
        var found = _breeds.TryGetValue(petId, out var breed);
        model = breed.Model;
        return found;
    }

    public static string? GetMaterialGroup(int petId, int? coat)
    {
        if (!_breeds.TryGetValue(petId, out var breed) || breed.Coats == 0)
            return null;
        return coat >= 1 && coat <= breed.Coats ? coat.Value.ToString() : "1";
    }

    // A model's material group is networked as a CUtlStringToken: MurmurHash2 of the lower-case
    // name, seeded with 0x31415926.
    private static readonly Dictionary<uint, string> _materialGroupNames = new[] { "default" }
        .Concat(Enumerable.Range(1, 13).Select(coat => coat.ToString()))
        .ToDictionary(name => MurmurHash2(name.ToLowerInvariant()), name => name);

    public static string GetMaterialGroupName(uint token) =>
        _materialGroupNames.TryGetValue(token, out var name) ? name : $"0x{token:x8}";

    private static uint MurmurHash2(string value)
    {
        const uint m = 0x5bd1e995;
        var data = System.Text.Encoding.UTF8.GetBytes(value);
        var length = data.Length;
        var hash = 0x31415926u ^ (uint)length;
        var index = 0;
        for (; length >= 4; index += 4, length -= 4)
        {
            var k = BitConverter.ToUInt32(data, index) * m;
            k ^= k >> 24;
            hash = (hash * m) ^ (k * m);
        }
        if (length == 3)
            hash ^= (uint)data[index + 2] << 16;
        if (length >= 2)
            hash ^= (uint)data[index + 1] << 8;
        if (length >= 1)
            hash = (hash ^ data[index]) * m;
        hash ^= hash >> 13;
        hash *= m;
        return hash ^ (hash >> 15);
    }

    public static string GetTargetName(ulong steamId) => $"{TargetNamePrefix}{steamId}";

    public static bool IsPetTargetName(string? name) =>
        name?.StartsWith(TargetNamePrefix, StringComparison.Ordinal) == true;
}
