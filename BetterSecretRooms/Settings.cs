using UnityModManagerNet;

namespace Chests
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool PlayAnimation { get; set; } = true;
        public int MaxNumberOfOrdinaryChests { get; set; } = 4;
        public int MaxNumberOfAdvancedChests { get; set; } = 2;
        public int MaxNumberOfEliteChests { get; set; } = 1;
        public float OrdinaryChestChance { get; set; } = 0.5f;
        public float AdvancedChestChance { get; set; } = 0.4f;
        public float EliteChestChance { get; set; } = 0.3f;
        //public int RewardLevel { get; set; } = 0;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}