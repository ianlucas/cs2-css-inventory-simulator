/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;

namespace InventorySimulator;

public static partial class Natives
{
    public static readonly MemoryFunctionWithReturn<
        nint,
        nint,
        int,
        nint,
        byte,
        nint,
        nint
    > CCSPlayer_ItemServices_GiveNamedItem = new(
        GameData.GetSignature("CCSPlayer_ItemServices::GiveNamedItem")
    );
}
