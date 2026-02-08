/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Buffers;
using System.Text;

namespace InventorySimulator;

public class CEconItemSchema(nint handle)
{
    public nint Handle { get; set; } = handle;
    public bool IsValid => Handle != 0;

    public CEconItemDefinition? GetItemDefinitionByName(string pchName)
    {
        var address = Natives.CEconItemSchema_GetItemDefinitionByName.Invoke(Handle, pchName);
        var itemDef = new CEconItemDefinition(address);
        return itemDef.IsValid ? itemDef : null;
    }

    public CEconItemDefinition? GetItemDefinition(uint defIndex)
    {
        var address = Natives.CEconItemSchema_GetItemDefinition.Invoke(Handle, defIndex, 0);
        var itemDef = new CEconItemDefinition(address);
        return itemDef.IsValid ? itemDef : null;
    }
}
