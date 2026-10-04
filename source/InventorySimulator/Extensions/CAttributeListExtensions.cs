/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core;

namespace InventorySimulator;

public static class CAttributeListExtensions
{
    extension(CAttributeList self)
    {
        public void SetOrAddAttributeValueByName(string name, float value)
        {
            Natives.CAttributeList_SetOrAddAttributeValueByName.Invoke(self.Handle, name, value);
        }
    }
}
