using System.ComponentModel;
using System.Collections.Generic;
using InventorySystem.Items;

namespace CleanMaster
{
    public class Config : Exiled.API.Interfaces.IConfig
    {
        [Description("Whether the plugin is enabled")]
        public bool IsEnabled { get; set; } = true;

        [Description("Debug mode (shows detailed logs)")]
        public bool Debug { get; set; } = true;

        [Description("Enable corpse cleanup")]
        public bool EnableCorpseCleanup { get; set; } = true;

        [Description("Corpse lifetime in seconds")]
        public float CorpseLifetime { get; set; } = 180f;

        [Description("Cleanup interval in seconds")]
        public float CleanupInterval { get; set; } = 60f;

        [Description("Initial delay after spawn in seconds")]
        public float InitialDelay { get; set; } = 120f;

        [Description("Clean on round end")]
        public bool CleanOnRoundEnd { get; set; } = true;

        [Description("Clean items after death")]
        public bool CleanItemsOnDeath { get; set; } = true;

        [Description("Item cleanup delay after death (seconds)")]
        public float ItemsCleanupDelay { get; set; } = 15f;

        [Description("Enable loose items cleanup")]
        public bool EnableItemCleanup { get; set; } = true;

        [Description("List of protected item types")]
        public List<ItemType> ProtectedItems { get; set; } = new List<ItemType>
        {
            ItemType.MicroHID,
            ItemType.KeycardO5,
            ItemType.KeycardFacilityManager,
            ItemType.KeycardContainmentEngineer,
            ItemType.KeycardScientist,
            ItemType.SCP330,
            ItemType.SCP268,
            ItemType.SCP207,
            ItemType.SCP1576,
            ItemType.Jailbird,
            ItemType.ParticleDisruptor,
            ItemType.Coin,
            ItemType.GrenadeFlash,
            ItemType.GrenadeHE,
            ItemType.Radio
        };

        [Description("Permission required to use the clean command")]
        public string CleanCommandPermission { get; set; } = "cleanmaster.clean";

        [Description("Enable C.A.S.S.I.E cleanup announcements")]
        public bool EnableCleanupAnnouncements { get; set; } = true;

        [Description("C.A.S.S.I.E announcement text")]
        public string CleanupAnnouncementText { get; set; } = "Cleanup complete. Removed {items} items and {corpses} corpses";
    }
}