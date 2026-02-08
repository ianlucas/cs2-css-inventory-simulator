/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;

namespace InventorySimulator;

public static partial class Natives
{
    public static readonly MemoryFunctionWithReturn<nint, nint, nint> CEconItemSchema_GetItemDefinitionByName = new(
        GameData.GetSignature("CEconItemSchema::GetItemDefinitionByName")
    );
}
