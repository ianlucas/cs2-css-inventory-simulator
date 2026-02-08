/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;

namespace InventorySimulator;

public static class ConVars
{
    public static readonly FakeConVar<string> Url = new(
        "invsim_url",
        "API URL for the Inventory Simulator service.",
        "https://inventory.cstrike.app"
    );

    public static readonly FakeConVar<string> ApiKey = new(
        "invsim_apikey",
        "API key for the Inventory Simulator service.",
        ""
    );

    public static readonly FakeConVar<string> File = new(
        "invsim_file",
        "Inventory data file to load when the plugin starts.",
        "inventories.json"
    );

    public static readonly FakeConVar<bool> IsWsEnabled = new(
        "invsim_ws_enabled",
        "Allow players to refresh their inventory using the !ws command.",
        false
    );

    public static readonly FakeConVar<bool> IsWsImmediately = new(
        "invsim_ws_immediately",
        "Apply skin changes immediately without requiring a respawn.",
        false
    );

    public static readonly FakeConVar<int> WsCooldown = new(
        "invsim_ws_cooldown",
        "Cooldown duration in seconds between inventory refreshes per player.",
        30
    );

    public static readonly FakeConVar<string> WsUrlPrintFormat = new(
        "invsim_ws_url_print_format",
        "URL format string displayed when using the !ws command.",
        "{Host}"
    );

    public static readonly FakeConVar<bool> IsWsLogin = new(
        "invsim_wslogin",
        "Allow players to authenticate with Inventory Simulator and display their login URL (not recommended).",
        false
    );

    public static readonly FakeConVar<bool> IsRequireInventory = new(
        "invsim_require_inventory",
        "Require the player's inventory to be fetched before allowing them to join the game.",
        false
    );

    public static readonly FakeConVar<bool> IsSprayEnabled = new(
        "invsim_spray_enabled",
        "Enable spraying via the !spray command and/or use key.",
        true
    );

    public static readonly FakeConVar<bool> IsSprayOnUse = new(
        "invsim_spray_on_use",
        "Apply spray when the player presses the use key.",
        false
    );

    public static readonly FakeConVar<int> SprayCooldown = new(
        "invsim_spray_cooldown",
        "Cooldown duration in seconds between sprays per player.",
        30
    );

    public static readonly FakeConVar<bool> IsSprayChangerEnabled = new(
        "invsim_spraychanger_enabled",
        "Replace the player's vanilla spray with their equipped graffiti.",
        false
    );

    public static readonly FakeConVar<bool> IsStatTrakIgnoreBots = new(
        "invsim_stattrak_ignore_bots",
        "Ignore StatTrak kill count increments for bot kills.",
        true
    );

    public static readonly FakeConVar<bool> IsFallbackTeam = new(
        "invsim_fallback_team",
        "Allow using skins from any team (prioritizes current team first).",
        false
    );

    public static readonly FakeConVar<int> MinModels = new(
        "invsim_minmodels",
        "Enable player agents (0 = enabled, 1 = use map models per team, 2 = SAS & Phoenix).",
        0
    );

    public static void Initialize(BasePlugin plugin)
    {
        plugin.RegisterFakeConVars(typeof(ConVars));
    }
}
