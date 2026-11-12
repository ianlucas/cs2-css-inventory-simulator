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
        // From CounterStrikeSharp's gamedata, it points at "mov reg, [rip + sm_pFirst]". Offsets
        // are read here, not from lazy fields, because this runs during the type's static
        // initialization.
        var signature = GameData.GetSignature("IGameSystem_InitAllSystems_pFirst");
        var address = NativeAPI.FindSignature(Addresses.ServerPath, signature);
        if (address == nint.Zero)
            throw new InvalidOperationException(
                "Failed to locate the game system list. The signature may be outdated."
            );
        var displacementOffset = GameData.GetOffset(
            "IGameSystem_InitAllSystems_pFirst::DisplacementOffset"
        );
        var instructionSize = GameData.GetOffset(
            "IGameSystem_InitAllSystems_pFirst::InstructionSize"
        );
        var m_pNext = GameData.GetOffset("CBaseGameSystemFactory::m_pNext");
        var m_pName = GameData.GetOffset("CBaseGameSystemFactory::m_pName");
        var getStaticGameSystem = GameData.GetOffset("IGameSystemFactory::GetStaticGameSystem");
        var first = address + instructionSize + Marshal.ReadInt32(address + displacementOffset);
        var factory = Marshal.ReadIntPtr(first);
        while (factory != nint.Zero)
        {
            if (Marshal.PtrToStringUTF8(Marshal.ReadIntPtr(factory + m_pName)) == name)
                return VirtualFunction.Create<nint, nint>(factory, getStaticGameSystem)(factory);
            factory = Marshal.ReadIntPtr(factory + m_pNext);
        }
        throw new InvalidOperationException(
            $"Failed to locate game system '{name}'. The game system may not exist in the current game version."
        );
    }
}
