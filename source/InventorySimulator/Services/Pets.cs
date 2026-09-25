/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;

namespace InventorySimulator;

// Round restarts keep every "chicken" entity, so a pet that loses its owner's tracking would live
// until the map changes. Everything here exists to keep one pet per player and nothing more.
public static class Pets
{
    private const uint EF_MARKED_FOR_DELETE = 0x200;
    private static readonly HashSet<uint> _queuedSpawns = [];

    public static bool IsMapUnloading { get; set; } = false;

    // Set on unload: callbacks queued by this load of the plugin must not spawn pets afterwards.
    public static bool IsUnloaded { get; set; } = false;

    public static bool CanHavePet(this CCSPlayerController self) =>
        self.IsValid
        && !self.IsBot
        && !self.IsHLTV
        && self.Connected == PlayerConnectedState.Connected
        && self.TeamNum >= (byte)CsTeam.Terrorist;

    // Spawn events, !ws and team changes can ask for the same pet in one frame; they collapse into
    // a single spawn on the next frame, for the same person only.
    public static void QueueSpawn(CCSPlayerController player)
    {
        var index = player.Index;
        var steamId = player.SteamID;
        if (!_queuedSpawns.Add(index))
            return;
        Server.NextWorldUpdate(() =>
        {
            _queuedSpawns.Remove(index);
            if (IsUnloaded)
                return;
            var current = Utilities.GetPlayerFromIndex((int)index);
            if (current == null || !current.IsValid || current.SteamID != steamId)
                return;
            try
            {
                current.SpawnPet();
            }
            catch (Exception error)
            {
                Runtime.Plugin.Logger.LogError(
                    "Could not spawn the pet of {SteamId}: {Message}",
                    steamId,
                    error.Message
                );
            }
        });
    }

    public static bool IsMarkedForDeletion(this CChicken self) =>
        (self.Entity!.Flags & EF_MARKED_FOR_DELETE) != 0;

    public static List<CChicken> FindChickens() =>
        Utilities
            .FindAllEntitiesByDesignerName<CChicken>("chicken")
            .Where(chicken =>
                chicken.IsValid && chicken.DesignerName == "chicken" && !chicken.IsMarkedForDeletion()
            )
            .ToList();

    public static bool IsTaggedPet(this CChicken self) => PetHelper.IsPetTargetName(self.Entity?.Name);

    // Keeps exactly the pets of players who may have one and removes every other chicken this
    // plugin created, including ones an earlier load of the plugin lost track of.
    public static void QueueSpawnAll()
    {
        foreach (var player in Utilities.GetPlayers())
            if (player.CanHavePet())
                QueueSpawn(player);
    }

    public static int Reconcile(string reason)
    {
        if (IsMapUnloading || IsUnloaded)
            return 0;
        var removed = 0;
        try
        {
            var tracked = new HashSet<uint>();
            foreach (var (index, controllerState) in CCSPlayerControllerExtensions.GetStates())
            {
                var pet = controllerState.GetPet();
                if (pet == null)
                {
                    controllerState.ForgetPet();
                    continue;
                }
                var player = Utilities.GetPlayerFromIndex((int)index);
                var isEligible =
                    ConVars.IsPetsEnabled.Value
                    && player != null
                    && player.CanHavePet()
                    && player.SteamID == controllerState.SteamID
                    && controllerState.Inventory?.Pet?.PetId != null;
                if (!isEligible)
                {
                    controllerState.RemovePet();
                    removed++;
                }
                else if (!tracked.Add(pet.EntityHandle.Raw))
                    controllerState.ForgetPet();
            }
            foreach (var chicken in FindChickens())
                if (chicken.IsTaggedPet() && !tracked.Contains(chicken.EntityHandle.Raw))
                {
                    chicken.Remove();
                    removed++;
                }
        }
        catch (Exception error)
        {
            Runtime.Plugin.Logger.LogError(
                "Could not check pets ({Reason}): {Message}",
                reason,
                error.Message
            );
        }
        if (removed > 0)
            Runtime.Plugin.Logger.LogWarning(
                "Removed {Count} stray pet(s) ({Reason}).",
                removed,
                reason
            );
        return removed;
    }

    public static List<string> Describe()
    {
        var lines = new List<string>();
        var owners = new Dictionary<uint, CCSPlayerControllerState>();
        foreach (var (_, controllerState) in CCSPlayerControllerExtensions.GetStates())
            if (controllerState.PetHandle is uint handle)
                owners[handle] = controllerState;
        var chickens = FindChickens();
        int pets = 0,
            orphans = 0,
            foreign = 0;
        foreach (var chicken in chickens)
        {
            var handle = chicken.EntityHandle.Raw;
            var owner = chicken.Owner.Value;
            var isTracked = owners.TryGetValue(handle, out var controllerState);
            var isTagged = chicken.IsTaggedPet();
            if (isTracked)
                pets++;
            else if (isTagged)
                orphans++;
            else if (owner != null)
                foreign++;
            if (!isTracked && !isTagged && owner == null)
                continue;
            var node = chicken.CBodyComponent?.SceneNode;
            lines.Add(
                $"#{chicken.Index} {(isTracked ? "pet" : isTagged ? "ORPHAN" : "FOREIGN")}"
                    + $" owner={owner?.PlayerName ?? "-"} ({controllerState?.SteamID.ToString() ?? "-"})"
                    + $" leader={chicken.Leader.Value?.OriginalController.Value?.PlayerName ?? "-"}"
                    + $" model={chicken.GetModelName()}"
                    + $" coat={PetHelper.GetMaterialGroupName(node?.GetSkeletonInstance().MaterialGroup.Value ?? 0)}"
                    + $" scale={node?.Scale ?? 0:0.##} hp={chicken.Health}"
                    + $" hash={controllerState?.PetHash ?? "-"}"
            );
        }
        var humans = Utilities.GetPlayers().Count(player => player.CanHavePet());
        lines.Add(
            $"chickens={chickens.Count} pets={pets} orphans={orphans} foreignOwned={foreign}"
                + $" eligiblePlayers={humans} enabled={ConVars.IsPetsEnabled.Value}"
        );
        if (orphans > 0 || pets > humans)
            lines.Add("VIOLATION: run css_invsim_pets fix");
        return lines;
    }
}
