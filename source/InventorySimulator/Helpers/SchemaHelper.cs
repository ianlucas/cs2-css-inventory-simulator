/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Runtime.InteropServices;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace InventorySimulator;

public static class SchemaHelper
{
    public static CEconItemView CreateCEconItemView(nint copyFrom = 0)
    {
        var ptr = Marshal.AllocHGlobal(Helper.GetSchemaSize<CEconItemView>());
        Natives.CEconItemView_Constructor.Call(ptr);
        if (copyFrom != 0)
            Natives.CEconItemView_OperatorEquals.Call(ptr, copyFrom);
        return Swiftly.Core.Memory.ToSchemaClass<CEconItemView>(ptr);
    }

    public static CEconItemSchema? GetItemSchema()
    {
        var ptr = Natives.GetItemSchema.Call();
        var schema = new CEconItemSchema(ptr);
        return schema.IsValid ? schema : null;
    }
}
