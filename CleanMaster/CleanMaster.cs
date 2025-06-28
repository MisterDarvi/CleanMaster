using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using InventorySystem.Items.Usables.Scp330;
using MEC;
using Mirror;
using PlayerStatsSystem;
using UnityEngine;

namespace CleanMaster
{
    public class CleanMaster : Plugin<Config>
    {
        public override string Name => "CleanMaster";
        public override string Prefix => "CM";
        public override string Author => "MrDarvi";
        public override Version Version => new Version(1, 0, 0);
        public override Version RequiredExiledVersion => new Version(9, 6, 1);

        private readonly HashSet<string> _protectedZones = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CHECKPOINT", "ARMORY", "LOCKER", "CAMERA",
            "096", "MICROHID", "POCKET", "SHELTER"
        };

        private CoroutineHandle _cleanupCoroutine;
        private bool _spawnDetected;

        public override void OnEnabled()
        {
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStart;
            Exiled.Events.Handlers.Server.RespawningTeam += OnTeamSpawn;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnd;
            Exiled.Events.Handlers.Player.Died += OnPlayerDeath;

            _cleanupCoroutine = Timing.RunCoroutine(CleanupLoop());
            Log.Info($"CleanMaster v{Version} activated");
        }

        private IEnumerator<float> CleanupLoop()
        {
            yield return Timing.WaitUntilTrue(() => _spawnDetected);
            yield return Timing.WaitForSeconds(Config.InitialDelay);

            while (true)
            {
                if (Config.EnableCorpseCleanup)
                {
                    CleanCorpses();
                }
                if (Config.EnableItemCleanup)
                {
                    CleanLooseItems();
                }
                yield return Timing.WaitForSeconds(Config.CleanupInterval);
            }
        }

        private void CleanCorpses()
        {
            int cleaned = 0;
            int total = Ragdoll.List.Count();

            if (Config.Debug)
            {
                Log.Debug($"Checking {total} ragdolls for cleanup (Lifetime: {Config.CorpseLifetime}s)");
            }

            foreach (var ragdoll in Ragdoll.List.ToList())
            {
                try
                {
                    if (ragdoll == null || ragdoll.GameObject == null)
                    {
                        if (Config.Debug) Log.Debug("Skipping null ragdoll");
                        continue;
                    }

                    if (Config.Debug)
                    {
                        Log.Debug($"Checking ragdoll: {ragdoll.Nickname}, " +
                                $"Created: {ragdoll.CreationTime}, " +
                                $"Age: {(DateTime.Now - ragdoll.CreationTime).TotalSeconds}s, " +
                                $"Room: {ragdoll.Room?.Name ?? "null"}");
                    }

                    if (ShouldCleanRagdoll(ragdoll))
                    {
                        if (Config.Debug) Log.Debug($"Destroying ragdoll: {ragdoll.Nickname}");
                        ragdoll.Destroy();
                        cleaned++;
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"Error cleaning ragdoll: {e}");
                }
            }

            if (cleaned > 0 || Config.Debug)
            {
                Log.Debug($"Cleaned {cleaned} of {total} corpses");
            }
        }

        private bool ShouldCleanRagdoll(Ragdoll ragdoll)
        {
            if (ragdoll == null || ragdoll.GameObject == null)
                return false;

            // Checking the time of existence
            var age = (DateTime.Now - ragdoll.CreationTime).TotalSeconds;
            if (age < Config.CorpseLifetime)
            {
                if (Config.Debug) Log.Debug($"Ragdoll too young: {age}s < {Config.CorpseLifetime}s");
                return false;
            }

            // Checking the protected area
            if (ragdoll.Room != null && IsInProtectedZone(ragdoll.Room))
            {
                if (Config.Debug) Log.Debug($"Ragdoll in protected zone: {ragdoll.Room.Name}");
                return false;
            }

            return true;
        }

        private void CleanLooseItems()
        {
            int cleaned = 0;
            foreach (var pickup in Pickup.List.ToList())
            {
                try
                {
                    if (ShouldCleanItem(pickup))
                    {
                        pickup.Destroy();
                        cleaned++;
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"Error cleaning item: {e}");
                }
            }
            if (cleaned > 0 && Config.Debug)
                Log.Debug($"Cleaned {cleaned} loose items");
        }

        private bool ShouldCleanItem(Pickup pickup)
        {
            if (pickup == null || pickup.Room == null)
                return false;

            if (pickup.PreviousOwner != null)
                return false;

            if (IsInProtectedZone(pickup.Room))
            {
                if (Config.Debug) Log.Debug($"Item in protected zone: {pickup.Room.Name}");
                return false;
            }

            return true;
        }

        private void OnPlayerDeath(DiedEventArgs ev)
        {
            if (!Config.CleanItemsOnDeath || ev.Player == null)
                return;

            Timing.CallDelayed(Config.ItemsCleanupDelay, () =>
            {
                int cleaned = 0;
                foreach (var pickup in Pickup.List.ToList())
                {
                    try
                    {
                        if (ShouldCleanPlayerItem(pickup, ev.Player))
                        {
                            pickup.Destroy();
                            cleaned++;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Error cleaning player item: {e}");
                    }
                }
                if (cleaned > 0 && Config.Debug)
                    Log.Debug($"Cleaned {cleaned} items from {ev.Player.Nickname}");
            });
        }

        private bool ShouldCleanPlayerItem(Pickup pickup, Player player)
        {
            if (pickup == null || pickup.Room == null)
                return false;

            if (pickup.PreviousOwner == null || pickup.PreviousOwner.Id != player.Id)
                return false;

            if (IsInProtectedZone(pickup.Room))
            {
                if (Config.Debug) Log.Debug($"Player item in protected zone: {pickup.Room.Name}");
                return false;
            }

            return true;
        }

        private bool IsInProtectedZone(Room room)
        {
            if (room == null)
                return false;

            return _protectedZones.Any(z => room.Name.IndexOf(z, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void OnRoundStart()
        {
            _spawnDetected = false;
            if (Config.Debug) Log.Debug("New round started");
        }

        private void OnTeamSpawn(RespawningTeamEventArgs ev)
        {
            if (!_spawnDetected)
            {
                _spawnDetected = true;
                if (Config.Debug) Log.Debug($"Team {ev.NextKnownTeam} spawned");
            }
        }

        private void OnRoundEnd(RoundEndedEventArgs ev)
        {
            if (Config.CleanOnRoundEnd)
            {
                CleanCorpses();
                CleanLooseItems();
            }
        }

        public override void OnDisabled()
        {
            Timing.KillCoroutines(_cleanupCoroutine);
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStart;
            Exiled.Events.Handlers.Server.RespawningTeam -= OnTeamSpawn;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnd;
            Exiled.Events.Handlers.Player.Died -= OnPlayerDeath;
        }
    }
}