/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Runtime.InteropServices;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;

namespace InventorySimulator;

public static partial class Natives
{
    private static nint GetGameSystemByName(string name)
    {
        // From CounterStrikeSharp's gamedata, it points at "mov reg, [rip + sm_pFirst]".
        var signature = GameData.GetSignature("IGameSystem_InitAllSystems_pFirst");
        var address = NativeAPI.FindSignature(Addresses.ServerPath, signature);
        if (address == nint.Zero)
            throw new InvalidOperationException(
                "Failed to locate the game system list. The signature may be outdated."
            );
        var first = address + 7 + Marshal.ReadInt32(address + 3);
        var factory = Marshal.ReadIntPtr(first);
        while (factory != nint.Zero)
        {
            // CBaseGameSystemFactory::m_pName
            if (Marshal.PtrToStringUTF8(Marshal.ReadIntPtr(factory + 0x10)) == name)
                // IGameSystemFactory::GetStaticGameSystem
                return VirtualFunction.Create<nint, nint>(factory, 9)(factory);
            // CBaseGameSystemFactory::m_pNext
            factory = Marshal.ReadIntPtr(factory + 0x8);
        }
        throw new InvalidOperationException(
            $"Failed to locate game system '{name}'. The game system may not exist in the current game version."
        );
    }
}
