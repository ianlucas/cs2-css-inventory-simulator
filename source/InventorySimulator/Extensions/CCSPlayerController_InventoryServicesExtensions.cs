/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core;

namespace InventorySimulator;

public static class CCSPlayerController_InventoryServicesExtensions
{
    public static CCSPlayerController? GetController(this CCSPlayer_ItemServices self)
    {
        var pawn = self.Pawn.Value;
        return pawn != null && pawn.Controller.Value != null ? pawn.Controller.Value.As<CCSPlayerController>() : null;
    }

    public static CCSPlayerInventory GetInventory(this CCSPlayerController_InventoryServices self)
    {
        return new CCSPlayerInventory(self.Handle + Natives.CCSPlayerController_InventoryServices_m_pInventory);
    }
}
