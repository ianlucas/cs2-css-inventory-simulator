/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace InventorySimulator;

// CS2's client caches generated skin materials by paint kit, seed and wear rounded to three decimals,
// regardless of the weapon. The first-person material is also generated on top of the weapon's
// current material, so a cached one keeps the stickers it was first generated with. Only the client
// can evict entries (regenerate_weapon_skins), so we give every weapon and sticker combination its own
// key, moving the wear to the nearest free 0.001 step when needed. Clients see every player's weapons
// and clear this cache on map change, so claims are shared by all players and reset on map load. Based
// on workarounds by @stefanx111 and @bklol.
public static class WearRegistry
{
    private const int MaxBucket = 1000;

    private static readonly Lock _lock = new();
    private static readonly Dictionary<
        (int Paint, int Seed, int Bucket),
        (ushort Def, string Stickers)
    > _claims = [];

    public static float? Resolve(InventoryItem item)
    {
        if (item is not { Def: ushort def, Paint: int paint })
            return null;
        var seed = item.Seed ?? 0;
        var wear = item.Wear ?? 0;
        var bucket = GetBucket(wear);
        var signature = GetSignature(def, item.Stickers);
        lock (_lock)
        {
            // Nearest bucket first: 0, +1, -1, +2, -2...
            for (var step = 0; step <= 2 * MaxBucket; step++)
            {
                var candidate = bucket + (step % 2 == 1 ? 1 : -1) * ((step + 1) / 2);
                if (candidate is < 0 or > MaxBucket)
                    continue;
                var key = (paint, seed, candidate);
                if (_claims.TryGetValue(key, out var claim) && claim != signature)
                    continue;
                _claims[key] = signature;
                return candidate == bucket ? wear : GetWearInBucket(wear, candidate);
            }
        }
        return wear;
    }

    // Starts over, keeping the wears already in use by the given items.
    public static void Reset(IEnumerable<InventoryItem> items)
    {
        lock (_lock)
        {
            _claims.Clear();
            foreach (var item in items)
                if (item is { Def: ushort def, Paint: int paint, WearOverride: float wear })
                    _claims.TryAdd(
                        (paint, item.Seed ?? 0, GetBucket(wear)),
                        GetSignature(def, item.Stickers)
                    );
        }
    }

    private static (ushort Def, string Stickers) GetSignature(
        ushort def,
        List<StickerItem>? stickers
    )
    {
        var layout = string.Join(
            "_",
            (stickers ?? [])
                .OrderBy(s => s.Slot)
                .Select(s => $"{s.Slot}:{s.Def}:{s.Schema}:{s.Wear}:{s.Rotation}:{s.X}:{s.Y}")
        );
        return (def, layout);
    }

    // The client formats wear with "%.3f". A float times 1000 is exact as a double, and printf rounds
    // ties to even.
    private static int GetBucket(float wear) =>
        (int)Math.Round(wear * 1000d, MidpointRounding.ToEven);

    // Moves the wear by whole 0.001 steps to keep its remaining digits, unless float rounding lands it
    // in another bucket or below zero (printed as "-0.000", a different key).
    private static float GetWearInBucket(float wear, int bucket)
    {
        var shifted = (float)(wear + (bucket - GetBucket(wear)) / 1000d);
        return shifted >= 0 && GetBucket(shifted) == bucket ? shifted : bucket / 1000f;
    }
}
