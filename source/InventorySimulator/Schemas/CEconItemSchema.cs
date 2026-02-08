/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Buffers;
using System.Text;
using SwiftlyS2.Shared.Natives;

namespace InventorySimulator;

public class CEconItemSchema(nint address) : INativeHandle
{
    public nint Address { get; set; } = address;
    public bool IsValid => Address != 0;

    public unsafe CEconItemDefinition? GetItemDefinitionByName(string pchName)
    {
        var pool = ArrayPool<byte>.Shared;
        var nameLength = Encoding.UTF8.GetByteCount(pchName);
        var nameBuffer = pool.Rent(nameLength + 1);
        try
        {
            _ = Encoding.UTF8.GetBytes(pchName, nameBuffer);
            nameBuffer[nameLength] = 0;
            fixed (byte* pName = nameBuffer)
            {
                var address = Natives.CEconItemSchema_GetItemDefinitionByName.Call(
                    Address,
                    (nint)pName
                );
                var itemDef = new CEconItemDefinition(address);
                return itemDef.IsValid ? itemDef : null;
            }
        }
        finally
        {
            pool.Return(nameBuffer);
        }
    }

    public CEconItemDefinition? GetItemDefinition(uint defIndex)
    {
        var address = Natives.CEconItemSchema_GetItemDefinition.Call(Address, defIndex, 0);
        var itemDef = new CEconItemDefinition(address);
        return itemDef.IsValid ? itemDef : null;
    }
}
