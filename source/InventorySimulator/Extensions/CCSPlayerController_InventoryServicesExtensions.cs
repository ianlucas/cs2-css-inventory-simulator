/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core;

namespace InventorySimulator;

public static class CCSPlayerController_InventoryServicesExtensions
{
    extension(CCSPlayerController_InventoryServices self)
    {
        public CCSPlayerInventory Inventory =>
            new(self.Handle + Natives.CCSPlayerController_InventoryServices_m_pInventory);
    }
}
