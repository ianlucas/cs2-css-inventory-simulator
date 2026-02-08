/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Numerics;
using System.Runtime.InteropServices;
using CounterStrikeSharp.API.Core;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace InventorySimulator;

public static class SchemaHelper
{
    public static CEconItemView CreateCEconItemView(nint copyFrom = 0)
    {
        var ptr = Marshal.AllocHGlobal(680);
        Natives.CEconItemView_Constructor.Invoke(ptr);
        if (copyFrom != 0)
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
}
