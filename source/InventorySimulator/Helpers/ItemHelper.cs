/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace InventorySimulator;

public static class ItemHelper
{
    public static bool IsMeleeDesignerName(string designerName)
    {
        return designerName.Contains("bayonet") || designerName.Contains("knife");
    }
}
