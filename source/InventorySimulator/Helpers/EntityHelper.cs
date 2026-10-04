/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;

namespace InventorySimulator;

public static class EntityHelper
{
    private static CHandle<CCSGameRulesProxy>? _gameRulesProxyHandle;

    public static CCSGameRules? GetGameRules()
    {
        var gameRulesProxy = _gameRulesProxyHandle?.Value;
        if (gameRulesProxy == null)
        {
            gameRulesProxy = Utilities
                .FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules")
                .FirstOrDefault();
            _gameRulesProxyHandle =
                gameRulesProxy != null ? new(gameRulesProxy.EntityHandle.Raw) : null;
        }
        return gameRulesProxy?.GameRules;
    }

    public static bool IsWarmupPeriod()
    {
        return GetGameRules()?.WarmupPeriod == true;
    }

    public static SpawnPoint? GetRandomSpawnPoint(byte team)
    {
        if (team != (byte)CsTeam.Terrorist && team != (byte)CsTeam.CounterTerrorist)
            return null;
        var spawnPoints = new List<SpawnPoint>();
        var randomSpawn = ConVar.Find("mp_randomspawn")?.GetPrimitiveValue<int>() ?? 0;
        if (randomSpawn == 1 || randomSpawn == team)
            spawnPoints = GetAllEnabledSpawnPoints("info_deathmatch_spawn");
        if (spawnPoints.Count == 0)
            spawnPoints = GetAllEnabledSpawnPoints(
                team == (byte)CsTeam.Terrorist
                    ? "info_player_terrorist"
                    : "info_player_counterterrorist"
            );
        return spawnPoints.Count > 0 ? spawnPoints[Random.Shared.Next(spawnPoints.Count)] : null;
    }

    private static List<SpawnPoint> GetAllEnabledSpawnPoints(string designerName)
    {
        return
        [
            .. Utilities
                .FindAllEntitiesByDesignerName<SpawnPoint>(designerName)
                .Where(spawnPoint => spawnPoint.Enabled),
        ];
    }
}
