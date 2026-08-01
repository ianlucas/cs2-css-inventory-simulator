/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API;
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
        Runtime.Initialize(this);
        ConVars.Initialize(this);
        RegisterListener<Listeners.OnEntityCreated>(OnEntityCreated);
        RegisterListener<Listeners.OnEntityDeleted>(OnEntityDeleted);
        RegisterEventHandler<EventPlayerConnect>(OnPlayerConnect, HookMode.Post);
        RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull, HookMode.Post);
        RegisterEventHandler<EventPlayerDeath>(OnPlayerDeathPre);
        RegisterEventHandler<EventRoundMvp>(OnRoundMvpPre);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect, HookMode.Post);
        Natives.CCSPlayerController_ProcessUsercmds.Hook(OnProcessUsercmds, HookMode.Post);
        VirtualFunctions.GiveNamedItemFunc.Hook(OnGiveNamedItemPre, HookMode.Pre);
        Natives.CCSPlayerInventory_GetItemInLoadout.Hook(GetItemInLoadout, HookMode.Post);
        ConVars.File.ValueChanged += OnFileChanged;
        ConVars.IsRequireInventory.ValueChanged += OnIsRequireInventoryChanged;
        ConVars.Url.ValueChanged += OnUrlChanged;
        ConVars.ApiKey.ValueChanged += OnApiSuspensionConVarChanged;
        ConVars.IsPublicApiStatTrakIncrement.ValueChanged += OnApiSuspensionConVarChanged;
        ConVars.IsPublicApiSprayConsume.ValueChanged += OnApiSuspensionConVarChanged;
        _lastUrl = ConVars.Url.Value;
        OnFileChanged(null, ConVars.File.Value);
        OnIsRequireInventoryChanged(null, ConVars.IsRequireInventory.Value);
    }

    private string _lastUrl = "";

    public void OnUrlChanged(object? _, string value)
    {
        Api.ResetSuspension();
        if (value == _lastUrl)
            return;
        _lastUrl = value;
        var isOfficialHost =
            Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && uri.Host.Equals("inventory.cstrike.app", StringComparison.OrdinalIgnoreCase);
        if (!isOfficialHost)
        {
            ConVars.IsPublicApiStatTrakIncrement.Value = false;
            ConVars.IsPublicApiSprayConsume.Value = false;
        }
    }

    public void OnApiSuspensionConVarChanged<T>(object? _, T value)
    {
        Api.ResetSuspension();
    }

    public void OnFileChanged(object? _, string value)
    {
        if (Inventories.Load(value))
            foreach (var player in Utilities.GetPlayers().Where(p => !p.IsBot))
                if (Inventories.TryGet(player.SteamID, out var inventory))
                    player.GetState().Inventory = inventory;
    }

    public void OnIsRequireInventoryChanged(object? _, bool value)
    {
        if (ConVars.IsRequireInventory.Value)
            Natives.CServerSideClientBase_ActivatePlayer.Hook(OnActivatePlayerPre, HookMode.Pre);
        else
            Natives.CServerSideClientBase_ActivatePlayer.Unhook(OnActivatePlayerPre, HookMode.Pre);
    }

    public override void Unload(bool hotReload)
    {
        CCSPlayerControllerState.ClearAllEconItemView();
    }
}
