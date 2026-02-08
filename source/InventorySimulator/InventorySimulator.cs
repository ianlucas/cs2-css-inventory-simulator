/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;

namespace InventorySimulator;

public partial class InventorySimulator : BasePlugin
{
    public override string ModuleAuthor => "Ian Lucas";
    public override string ModuleDescription => "Inventory Simulator (inventory.cstrike.app)";
    public override string ModuleName => "InventorySimulator";
    public override string ModuleVersion => "1.0.0";

    public override void Load(bool hotReload)
    {
        CSS.Initialize(this);
        ConVars.Initialize();
        RegisterListener<Listeners.OnEntityCreated>(OnEntityCreated);
        RegisterListener<Listeners.OnEntityDeleted>(OnEntityDeleted);
        RegisterEventHandler<EventPlayerConnect>(OnPlayerConnect, HookMode.Post);
        RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull, HookMode.Post);
        RegisterEventHandler<EventPlayerDeath>(OnPlayerDeathPre);
        RegisterEventHandler<EventRoundMvp>(OnRoundMvpPre);
        Natives.CCSPlayerController_ProcessUsercmds.Hook(OnProcessUsercmds, HookMode.Post);
        VirtualFunctions.GiveNamedItemFunc.Hook(OnGiveNamedItemPre, HookMode.Pre);
        Natives.CCSPlayerInventory_GetItemInLoadout.Hook(GetItemInLoadout, HookMode.Pre);
        ConVars.File.ValueChanged += HandleFileChanged;
        ConVars.IsRequireInventory.ValueChanged += HandleIsRequireInventoryChanged;
        HandleFileChanged(null, ConVars.File.Value);
        HandleIsRequireInventoryChanged(null, ConVars.IsRequireInventory.Value);
    }

    public override void Unload(bool hotReload)
    {
        CCSPlayerControllerState.ClearAllEconItemView();
    }
}
