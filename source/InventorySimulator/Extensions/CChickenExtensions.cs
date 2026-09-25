/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core;

namespace InventorySimulator;

public static class CChickenExtensions
{
    public static string GetModelName(this CChicken self) =>
        self.CBodyComponent?.SceneNode?.GetSkeletonInstance().ModelState.ModelName ?? "";

    // CChicken's spawn always sets the default chicken model and rolls a random coat, whatever the
    // pet item says, so the pet's own look is applied afterwards. The scale it took from the
    // item's upgrade level stays on the scene node.
    public static void ApplyPetLook(this CChicken self, string model, string? materialGroup)
    {
        if (!self.GetModelName().Equals(model, StringComparison.OrdinalIgnoreCase))
            self.SetModel(model);
        if (materialGroup != null)
            self.AcceptInput("SetMaterialGroup", self, self, materialGroup);
    }
}
