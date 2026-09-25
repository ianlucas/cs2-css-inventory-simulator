/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Timers;
using Microsoft.Extensions.Logging;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

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
        RegisterListener<Listeners.OnMapStart>(OnMapStart);
        RegisterListener<Listeners.OnMapEnd>(OnMapEnd);
        RegisterListener<Listeners.OnClientDisconnect>(OnClientDisconnect);
        RegisterEventHandler<EventPlayerConnect>(OnPlayerConnect, HookMode.Post);
        RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull, HookMode.Post);
        RegisterEventHandler<EventPlayerDeath>(OnPlayerDeathPre);
        RegisterEventHandler<EventRoundMvp>(OnRoundMvpPre);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect, HookMode.Post);
        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn, HookMode.Post);
        RegisterEventHandler<EventPlayerTeam>(OnPlayerTeam, HookMode.Post);
        RegisterEventHandler<EventRoundPrestart>(OnRoundPrestart, HookMode.Post);
        RegisterEventHandler<EventRoundStart>(OnRoundStart, HookMode.Post);
        VirtualFunctions.GiveNamedItemFunc.Hook(OnGiveNamedItemPre, HookMode.Pre);
        Natives.CCSPlayerInventory_GetItemInLoadout.Hook(GetItemInLoadout, HookMode.Post);
        ConVars.File.ValueChanged += OnFileChanged;
        ConVars.IsRequireInventory.ValueChanged += OnIsRequireInventoryChanged;
        ConVars.IsSprayOnUse.ValueChanged += OnIsSprayOnUseChanged;
        ConVars.Url.ValueChanged += OnUrlChanged;
        ConVars.ApiKey.ValueChanged += OnApiSuspensionConVarChanged;
        ConVars.IsPublicApiStatTrakIncrement.ValueChanged += OnApiSuspensionConVarChanged;
        ConVars.IsPublicApiSprayConsume.ValueChanged += OnApiSuspensionConVarChanged;
        ConVars.IsPetsEnabled.ValueChanged += OnIsPetsEnabledChanged;
        _lastUrl = ConVars.Url.Value;
        OnFileChanged(null, ConVars.File.Value);
        OnIsRequireInventoryChanged(null, ConVars.IsRequireInventory.Value);
        OnIsSprayOnUseChanged(null, ConVars.IsSprayOnUse.Value);
        // Deathmatch and practice never restart a round, so strays are also swept on a timer.
        _petsTimer = AddTimer(30, () => Pets.Reconcile("periodic check"), TimerFlags.REPEAT);
        if (hotReload)
            Server.NextWorldUpdate(() =>
            {
                Pets.Reconcile("plugin load");
                Pets.QueueSpawnAll();
            });
    }

    private Timer? _petsTimer;

    private string _lastUrl = "";
    private bool _isActivatePlayerHooked = false;
    private bool _isProcessUsercmdsHooked = false;

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
        if (value == _isActivatePlayerHooked)
            return;
        if (value)
            Natives.CServerSideClientBase_ActivatePlayer.Hook(OnActivatePlayerPre, HookMode.Pre);
        else
            Natives.CServerSideClientBase_ActivatePlayer.Unhook(OnActivatePlayerPre, HookMode.Pre);
        _isActivatePlayerHooked = value;
    }

    public void OnIsSprayOnUseChanged(object? _, bool value)
    {
        if (value == _isProcessUsercmdsHooked)
            return;
        if (value)
            Natives.CCSPlayerController_ProcessUsercmds.Hook(OnProcessUsercmds, HookMode.Post);
        else
            Natives.CCSPlayerController_ProcessUsercmds.Unhook(OnProcessUsercmds, HookMode.Post);
        _isProcessUsercmdsHooked = value;
    }

    public void OnIsPetsEnabledChanged(object? _, bool value)
    {
        if (!value)
        {
            CCSPlayerControllerExtensions.RemoveAllPets();
            Pets.Reconcile("pets disabled");
        }
        else
            Pets.QueueSpawnAll();
    }

    public override void Unload(bool hotReload)
    {
        // Pets go first: an unhook that throws must not leave chickens behind.
        try
        {
            Pets.IsUnloaded = true;
            _petsTimer?.Kill();
            CCSPlayerControllerExtensions.RemoveAllPets();
            foreach (var chicken in Pets.FindChickens())
                if (chicken.IsTaggedPet())
                    chicken.Remove();
        }
        catch (Exception error)
        {
            Logger.LogError("Could not remove pets on unload: {Message}", error.Message);
        }
        VirtualFunctions.GiveNamedItemFunc.Unhook(OnGiveNamedItemPre, HookMode.Pre);
        Natives.CCSPlayerInventory_GetItemInLoadout.Unhook(GetItemInLoadout, HookMode.Post);
        OnIsRequireInventoryChanged(null, false);
        OnIsSprayOnUseChanged(null, false);
        CCSPlayerControllerState.ClearAllEconItemView();
    }
}
