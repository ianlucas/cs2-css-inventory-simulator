invsim_url "https://inventory.cstrike.app"
    API URL for the Inventory Simulator service.

invsim_apikey ""
    API key for the Inventory Simulator service.

invsim_file "inventories.json"
    Inventory data file to load when the plugin starts.

invsim_ws_enabled false
    Allow players to refresh their inventory using the !ws command.

invsim_ws_immediately false
    Apply skin changes immediately without requiring a respawn.

invsim_ws_cooldown 30
    Cooldown duration in seconds between inventory refreshes per player.

invsim_ws_url_print_format "{Host}"
    URL format string displayed when using the !ws command.

invsim_wslogin false
    Allow players to authenticate with Inventory Simulator and display their login URL (not recommended).

invsim_require_inventory false
    Require the player's inventory to be fetched before allowing them to join the game.

invsim_spray_enabled true
    Enable spraying via the !spray command and/or use key.

invsim_spray_on_use false
    Apply spray when the player presses the use key.

invsim_spray_cooldown 30
    Cooldown duration in seconds between sprays per player.

invsim_spraychanger_enabled false
    Replace the player's vanilla spray with their equipped graffiti.

invsim_stattrak_ignore_bots true
    Ignore StatTrak kill count increments for bot kills.

invsim_fallback_team false
    Allow using skins from any team (prioritizes current team first).

invsim_minmodels 0
    Enable player agents (0 = enabled, 1 = use map models per team, 2 = SAS & Phoenix).

css_ws
    Refreshes player inventory from the Inventory Simulator service and displays the configured URL.

css_spray
    Applies the player's equipped graffiti spray at their current location.

css_wslogin
    Authenticates the player with Inventory Simulator and displays their login URL.