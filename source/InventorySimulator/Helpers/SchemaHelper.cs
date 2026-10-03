/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace InventorySimulator;

public static class SchemaHelper
{
    private static nint _emptyCEconItemView;

    public static nint GetEmptyCEconItemView()
    {
        if (_emptyCEconItemView == nint.Zero)
        {
            var itemView = CreateCEconItemView();
            itemView.Initialized = false;
            _emptyCEconItemView = itemView.Handle;
        }
        return _emptyCEconItemView;
    }

    public static void FreeEmptyCEconItemView()
    {
        if (_emptyCEconItemView == nint.Zero)
            return;
        Marshal.FreeHGlobal(_emptyCEconItemView);
        _emptyCEconItemView = nint.Zero;
    }

    public static CEconItemView CreateCEconItemView(nint copyFrom = 0)
    {
        var ptr = Marshal.AllocHGlobal(Schema.GetClassSize("CEconItemView"));
        Natives.CEconItemView_Constructor.Invoke(ptr);
        if (copyFrom != nint.Zero)
            Natives.CEconItemView_OperatorEquals.Invoke(ptr, copyFrom);
        return new CEconItemView(ptr);
    }

    public static CEconItemSchema? GetItemSchema()
    {
        var ptr = Natives.GetItemSchema.Invoke();
        var schema = new CEconItemSchema(ptr);
        return schema.IsValid ? schema : null;
    }

    public static Vector ToVector(Vector3 vec)
    {
        return new(vec.X, vec.Y, vec.Z);
    }

    public static uint MakeStringToken(string str)
    {
        const uint m = 0x5bd1e995;
        var data = Encoding.UTF8.GetBytes(str.ToLowerInvariant());
        var h = 0x31415926u ^ (uint)data.Length;
        var i = 0;
        for (; data.Length - i >= 4; i += 4)
        {
            var k = BitConverter.ToUInt32(data, i) * m;
            k ^= k >> 24;
            h = (h * m) ^ (k * m);
        }
        switch (data.Length - i)
        {
            case 3:
                h ^= (uint)data[i + 2] << 16;
                goto case 2;
            case 2:
                h ^= (uint)data[i + 1] << 8;
                goto case 1;
            case 1:
                h = (h ^ data[i]) * m;
                break;
        }
        h ^= h >> 13;
        h *= m;
        return h ^ (h >> 15);
    }
}
