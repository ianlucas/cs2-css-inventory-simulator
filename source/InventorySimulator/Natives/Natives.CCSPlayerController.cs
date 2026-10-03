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
        bool,
        float
    > CCSPlayerController_ProcessUsercmds = new(
        GameData.GetSignature("CCSPlayerController::ProcessUsercmds")
    );

    public static readonly MemoryFunctionWithReturn<nint, nint> CCSPlayerController_GetPetChicken =
        new(GameData.GetSignature("CCSPlayerController::GetPetChicken"));

    public static readonly MemoryFunctionVoid<nint, nint> CCSPlayerController_SetPetChicken = new(
        GameData.GetSignature("CCSPlayerController::SetPetChicken")
    );
}
