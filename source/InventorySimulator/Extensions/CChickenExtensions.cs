/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Numerics;
using System.Runtime.InteropServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;

namespace InventorySimulator;

public static class CChickenExtensions
{
    extension(CChicken)
    {
        // Mirrors how the game spawns pets on round_start.
        public static CChicken? CreatePet(
            CCSPlayerController controller,
            Vector3 position,
            Vector3? angles
        )
        {
            var inventory = controller.InventoryServices?.GetInventory();
            if (inventory?.IsValid != true)
                return null;
            // InitPet doesn't check that the pet slot is equipped, the game does it before calling.
            var itemView = inventory.GetItemInLoadout(0, loadout_slot_t.LOADOUT_SLOT_PET);
            if (itemView == nint.Zero || !new CEconItemView(itemView).Initialized)
                return null;
            var chicken = Utilities.CreateEntityByName<CChicken>("chicken");
            if (chicken == null)
                return null;
            if (!Natives.CChicken_InitPet.Invoke(chicken.Handle, controller.Handle))
            {
                chicken.Remove();
                return null;
            }
            controller.SetPetChicken(chicken);
            chicken.Teleport(position, angles);
            chicken.DispatchSpawn();
            return chicken;
        }
    }

    // The game stops pets from roaming shortly after freeze time ends.
    public static bool CanRoam(this CChicken self)
    {
        return Marshal.ReadByte(self.Handle + Natives.CChicken_m_bCanRoam) != 0;
    }

    public static void SetCanRoam(this CChicken self, bool value)
    {
        Marshal.WriteByte(self.Handle + Natives.CChicken_m_bCanRoam, (byte)(value ? 1 : 0));
    }

    public static void ApplyPetStyle(this CChicken self, InventoryItem item)
    {
        var skeletonInstance = self.CBodyComponent?.SceneNode?.GetSkeletonInstance();
        if (skeletonInstance == null)
            return;
        var materialGroup =
            item.Style > 0 ? SchemaHelper.MakeStringToken(item.Style.Value.ToString()) : 0;
        if (skeletonInstance.MaterialGroup.Value == materialGroup)
            return;
        skeletonInstance.MaterialGroup.Value = materialGroup;
        NativeAPI.SchemaSetStateChanged(
            skeletonInstance.Handle,
            (uint)Schema.GetSchemaOffset("CSkeletonInstance", "m_materialGroup"),
            0xFFFFFFFF,
            0xFFFFFFFF
        );
    }
}
