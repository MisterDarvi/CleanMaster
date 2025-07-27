using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using InventorySystem.Items;
using MEC;
using NorthwoodLib;
using PlayerStatsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CleanMaster
{
    public class CleanMaster : Plugin<Config>
    {
        private static CleanMaster _singleton;
        public static CleanMaster Singleton => _singleton;

        public override string Name => "CleanMaster";
        public override string Prefix => "CleanMaster";
        public override string Author => "MrDarvi";
        public override Version Version => new Version(1, 0, 1);
        public override Version RequiredExiledVersion => new Version(9, 6, 1);

        private readonly HashSet<string> _protectedZones = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CHECKPOINT", "ARMORY", "LOCKER", "CAMERA",
            "096", "MICROHID", "POCKET", "SHELTER"
        };

        private CoroutineHandle _cleanupCoroutine;
        private bool _spawnDetected;
        private bool _isCleanupEnabled = true;

        public bool IsCleanupEnabled
        {
            get => _isCleanupEnabled;
            set => _isCleanupEnabled = value;
        }

        public CleanMaster()
        {
            _singleton = this;
        }

        public override void OnEnabled()
        {
            if (_singleton != this)
            {
                Log.Warn("CleanMaster is already enabled!");
                return;
            }

            RegisterEvents();
            _cleanupCoroutine = Timing.RunCoroutine(CleanupRoutine());
            Log.Info($"CleanMaster v{Version} activated");
        }

        private void RegisterEvents()
        {
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            Exiled.Events.Handlers.Server.RespawningTeam += OnRespawningTeam;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
            Exiled.Events.Handlers.Player.Died += OnPlayerDied;
        }

        private void UnregisterEvents()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            Exiled.Events.Handlers.Server.RespawningTeam -= OnRespawningTeam;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;
            Exiled.Events.Handlers.Player.Died -= OnPlayerDied;
        }

        private IEnumerator<float> CleanupRoutine()
        {
            yield return Timing.WaitUntilTrue(() => _spawnDetected);
            yield return Timing.WaitForSeconds(Config.InitialDelay);

            while (true)
            {
                if (_isCleanupEnabled)
                {
                    if (Config.EnableCorpseCleanup)
                        CleanCorpses();

                    if (Config.EnableItemCleanup)
                        CleanItems();
                }
                yield return Timing.WaitForSeconds(Config.CleanupInterval);
            }
        }

        private void CleanCorpses()
        {
            int cleaned = 0;
            var corpses = Ragdoll.List.ToList();

            if (Config.Debug)
                Log.Debug($"Checking {corpses.Count} corpses for cleanup");

            foreach (var corpse in corpses)
            {
                try
                {
                    if (corpse == null || !ShouldCleanCorpse(corpse))
                        continue;

                    corpse.Destroy();
                    cleaned++;
                }
                catch (Exception e)
                {
                    Log.Error($"Error cleaning corpse: {e}");
                }
            }

            if (cleaned > 0 || Config.Debug)
                Log.Debug($"Cleaned {cleaned} of {corpses.Count} corpses");
        }

        private bool ShouldCleanCorpse(Ragdoll corpse)
        {
            if (corpse?.GameObject == null)
                return false;

            var age = (DateTime.Now - corpse.CreationTime).TotalSeconds;
            if (age < Config.CorpseLifetime)
            {
                if (Config.Debug)
                    Log.Debug($"Corpse too young: {age}s < {Config.CorpseLifetime}s");
                return false;
            }

            if (corpse.Room != null && IsProtectedZone(corpse.Room))
            {
                if (Config.Debug)
                    Log.Debug($"Corpse in protected zone: {corpse.Room.Name}");
                return false;
            }

            return true;
        }

        private void CleanItems()
        {
            int cleaned = 0;
            foreach (var item in Pickup.List.ToList())
            {
                try
                {
                    if (ShouldCleanItem(item))
                    {
                        item.Destroy();
                        cleaned++;
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"Error cleaning item: {e}");
                }
            }

            if (cleaned > 0 && Config.Debug)
                Log.Debug($"Cleaned {cleaned} items");
        }

        private bool ShouldCleanItem(Pickup item)
        {
            if (item?.Room == null)
                return false;

            if (Config.ProtectedItems?.Contains(item.Type) == true)
            {
                if (Config.Debug)
                    Log.Debug($"Item protected by config: {item.Type}");
                return false;
            }

            if (item.PreviousOwner != null || IsProtectedZone(item.Room))
                return false;

            return true;
        }

        private void OnPlayerDied(DiedEventArgs ev)
        {
            if (!Config.CleanItemsOnDeath || ev.Player == null)
                return;

            Timing.CallDelayed(Config.ItemsCleanupDelay, () =>
            {
                int cleaned = 0;
                foreach (var item in Pickup.List.ToList())
                {
                    try
                    {
                        if (ShouldCleanPlayerItem(item, ev.Player))
                        {
                            item.Destroy();
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

        private bool ShouldCleanPlayerItem(Pickup item, Player player)
        {
            if (item?.Room == null)
                return false;

            if (Config.ProtectedItems?.Contains(item.Type) == true)
            {
                if (Config.Debug)
                    Log.Debug($"Player item protected: {item.Type}");
                return false;
            }

            if (item.PreviousOwner?.Id != player.Id || IsProtectedZone(item.Room))
                return false;

            return true;
        }

        private bool IsProtectedZone(Room room)
        {
            return room != null && _protectedZones.Any(z =>
                room.Name.Contains(z, StringComparison.OrdinalIgnoreCase));
        }

        private void OnRoundStarted()
        {
            _spawnDetected = false;
            _isCleanupEnabled = true;
            if (Config.Debug)
                Log.Debug("New round started");
        }

        private void OnRespawningTeam(RespawningTeamEventArgs ev)
        {
            if (!_spawnDetected)
            {
                _spawnDetected = true;
                if (Config.Debug)
                    Log.Debug($"Team {ev.NextKnownTeam} spawned");
            }
        }

        private void OnRoundEnded(RoundEndedEventArgs ev)
        {
            if (Config.CleanOnRoundEnd && _isCleanupEnabled)
            {
                CleanCorpses();
                CleanItems();
            }
        }

        public override void OnDisabled()
        {
            if (_singleton != this)
                return;

            Timing.KillCoroutines(_cleanupCoroutine);
            UnregisterEvents();
            _singleton = null;
            Log.Info("CleanMaster disabled");
        }
    }
}