/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace InventorySimulator;

public class CSteamID(ulong ulSteamID)
{
    public ulong m_SteamID = ulSteamID;

    public AccountID_t GetAccountID()
    {
        return new AccountID_t((uint)(m_SteamID & 0xFFFFFFFFul));
    }
}
