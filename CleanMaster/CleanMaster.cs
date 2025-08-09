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
        public override Version Version => new Version(1, 0, 2);
        public override Version RequiredExiledVersion => new Version(9, 7, 2);

        private readonly HashSet<string> _protectedZones = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CHECKPOINT", "ARMORY", "LCZ", "HCZ", "EZ",
            "049", "106", "173", "096", "939",
            "MICROHID", "POCKET", "SHELTER", "NUKE",
            "GATE", "SURFACE", "TUNNEL", "ELEVATOR"
        };

        private CoroutineHandle _cleanupCoroutine;
        private bool _isCleanupEnabled = true;
        private int _lastCleanedItems;
        private int _lastCleanedCorpses;

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
            Log.Debug("=== Plugin Startup ===");

            // Register events
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            Exiled.Events.Handlers.Server.RespawningTeam += OnRespawningTeam;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
            Exiled.Events.Handlers.Player.Died += OnPlayerDied;

            // Start cleanup routine
            _cleanupCoroutine = Timing.RunCoroutine(CleanupRoutine());
            Log.Info($"CleanMaster v{Version} loaded successfully!");
        }

        private IEnumerator<float> CleanupRoutine()
        {
            yield return Timing.WaitForSeconds(Config.InitialDelay);

            while (true)
            {
                if (_isCleanupEnabled)
                {
                    // Corpse cleanup
                    if (Config.EnableCorpseCleanup)
                    {
                        _lastCleanedCorpses = 0;
                        var corpses = Exiled.API.Features.Ragdoll.List.ToList();
                        foreach (var corpse in corpses)
                        {
                            if (ShouldCleanCorpse(corpse))
                            {
                                corpse.Destroy();
                                _lastCleanedCorpses++;
                            }
                        }
                        Log.Debug($"Cleaned {_lastCleanedCorpses} corpses");
                    }

                    // Item cleanup
                    if (Config.EnableItemCleanup)
                    {
                        _lastCleanedItems = 0;
                        var items = Pickup.List.ToList();
                        foreach (var item in items)
                        {
                            if (ShouldCleanItem(item))
                            {
                                item.Destroy();
                                _lastCleanedItems++;
                            }
                        }
                        Log.Debug($"Cleaned {_lastCleanedItems} items");
                    }

                    // Announcement
                    if (Config.EnableCleanupAnnouncements &&
                       (_lastCleanedItems > 0 || _lastCleanedCorpses > 0))
                    {
                        Cassie.Message(Config.CleanupAnnouncementText
                            .Replace("{items}", _lastCleanedItems.ToString())
                            .Replace("{corpses}", _lastCleanedCorpses.ToString()),
                            isHeld: false, isNoisy: false);
                    }
                }
                yield return Timing.WaitForSeconds(Config.CleanupInterval);
            }
        }

        private bool ShouldCleanCorpse(Exiled.API.Features.Ragdoll corpse)
        {
            if (corpse?.GameObject == null) return false;
            if ((DateTime.Now - corpse.CreationTime).TotalSeconds < Config.CorpseLifetime) return false;
            if (corpse.Room != null && IsProtectedZone(corpse.Room)) return false;
            return true;
        }

        private bool ShouldCleanItem(Pickup item)
        {
            if (item?.Room == null || !item.IsSpawned) return false;
            if (Config.ProtectedItems.Contains(item.Type)) return false;
            if (IsProtectedZone(item.Room)) return false;
            return true;
        }

        private bool IsProtectedZone(Room room)
        {
            if (room == null) return false;
            return _protectedZones.Any(z => room.Name.Contains(z, StringComparison.OrdinalIgnoreCase)) ||
                   room.Type == RoomType.HczArmory ||
                   room.Type == RoomType.HczNuke ||
                   room.Type == RoomType.LczArmory;
        }

        private void OnRoundStarted() => _isCleanupEnabled = true;

        private void OnRespawningTeam(RespawningTeamEventArgs ev) => Log.Debug($"Team spawned: {ev.NextKnownTeam}");

        private void OnRoundEnded(RoundEndedEventArgs ev)
        {
            if (Config.CleanOnRoundEnd)
            {
                Timing.RunCoroutine(CleanupRoutine());
            }
        }

        private void OnPlayerDied(DiedEventArgs ev)
        {
            if (!Config.CleanItemsOnDeath) return;

            Timing.CallDelayed(Config.ItemsCleanupDelay, () =>
            {
                int cleaned = 0;
                foreach (var item in Pickup.List.Where(p => p.PreviousOwner?.Id == ev.Player.Id))
                {
                    if (ShouldCleanItem(item))
                    {
                        item.Destroy();
                        cleaned++;
                    }
                }
                Log.Debug($"Cleaned {cleaned} items from {ev.Player.Nickname}'s death");
            });
        }

        public override void OnDisabled()
        {
            Timing.KillCoroutines(_cleanupCoroutine);

            // Unregister events
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            Exiled.Events.Handlers.Server.RespawningTeam -= OnRespawningTeam;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;
            Exiled.Events.Handlers.Player.Died -= OnPlayerDied;

            _singleton = null;
            Log.Info("CleanMaster disabled");
        }
    }
}