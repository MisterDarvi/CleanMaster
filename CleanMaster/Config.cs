using System.ComponentModel;
using System.Collections.Generic;

namespace CleanMaster
{
    public class Config : Exiled.API.Interfaces.IConfig
    {
        [Description("Enable plugin")]
        public bool IsEnabled { get; set; } = true;

        [Description("Debug mode")]
        public bool Debug { get; set; } = false;

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

        [Description("List of item types that should never be cleaned (e.g. MicroHID, KeycardO5)")]
        public List<ItemType> ProtectedItems { get; set; } = new List<ItemType>
        {
            ItemType.MicroHID,
            ItemType.KeycardO5,
            ItemType.SCP330,
            ItemType.SCP268,
            ItemType.SCP207,
            ItemType.SCP1576, // SCP-1344
            ItemType.Jailbird,
            ItemType.ParticleDisruptor
        };

        [Description("Permission required to use the clean command")]
        public string CleanCommandPermission { get; set; } = "cleanmaster.clean";
    }
}