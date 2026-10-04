/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace InventorySimulator;

public static class CCSPlayerPawnExtensions
{
    extension(CCSPlayerPawn self)
    {
        public bool IsAbleToApplySpray(IntPtr ptr = 0)
        {
            return Natives.CCSPlayerPawn_IsAbleToApplySpray.Invoke(self.Handle, ptr, 0, 0)
                == nint.Zero;
        }

        public void SetModelFromLoadout()
        {
            Natives.CCSPlayerPawn_SetModelFromLoadout.Invoke(self.Handle);
        }

        public void SetModelFromClass()
        {
            Natives.CCSPlayerPawn_SetModelFromClass.Invoke(self.Handle);
        }

        public void RefreshGloves(bool hasGloves)
        {
            self.AcceptInput("SetBodygroup", value: "first_or_third_person,0");
            Server.NextWorldUpdate(() =>
            {
                if (!self.IsValid)
                    return;
                self.ItemServices?.As<CCSPlayer_ItemServices>().UpdateWearables();
                if (!hasGloves)
                    return;
                self.EconGlovesChanged++;
                Utilities.SetStateChanged(self, "CCSPlayerPawn", "m_nEconGlovesChanged");
            });
        }
    }
}
