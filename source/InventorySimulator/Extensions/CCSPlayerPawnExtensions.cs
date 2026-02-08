/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core;

namespace InventorySimulator;

public static class CCSPlayerPawnExtensions
{
    public static bool IsAbleToApplySpray(this CCSPlayerPawn self, IntPtr ptr = 0)
    {
        return Natives.CCSPlayerPawn_IsAbleToApplySpray.Invoke(self.Handle, ptr, 0, 0) == nint.Zero;
    }

    public static void SetModelFromLoadout(this CCSPlayerPawn self)
    {
        Natives.CCSPlayerPawn_SetModelFromLoadout.Invoke(self.Handle);
    }

    public static void SetModelFromClass(this CCSPlayerPawn self)
    {
        Natives.CCSPlayerPawn_SetModelFromClass.Invoke(self.Handle);
    }
}
