using InventorySystem.Items;

namespace CleanMaster
{
    public static class Extensions
    {
        public static bool IsKeycard(this ItemType itemType)
        {
            return itemType == ItemType.KeycardJanitor ||
                   itemType == ItemType.KeycardScientist ||
                   itemType == ItemType.KeycardResearchCoordinator ||
                   itemType == ItemType.KeycardZoneManager ||
                   itemType == ItemType.KeycardGuard ||
                   itemType == ItemType.KeycardContainmentEngineer ||
                   itemType == ItemType.KeycardFacilityManager ||
                   itemType == ItemType.KeycardO5;
        }

        public static bool IsSCPItem(this ItemType itemType)
        {
            return itemType == ItemType.SCP018 ||
                   itemType == ItemType.SCP207 ||
                   itemType == ItemType.SCP268 ||
                   itemType == ItemType.SCP330 ||
                   itemType == ItemType.SCP500 ||
                   itemType == ItemType.SCP1576 ||
                   itemType == ItemType.SCP244a ||
                   itemType == ItemType.SCP244b;
        }
    }
}