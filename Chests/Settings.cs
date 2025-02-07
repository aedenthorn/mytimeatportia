using UnityModManagerNet;

namespace Chests
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool PlayAnimation { get; set; } = true;
        public int NumberOfOrdinaryChests { get; set; } = 100;
        public int NumberOfAdvancedChests { get; set; } = 60;
        public int NumberOfEliteChests { get; set; } = 20;
        public int RespawnFrequency { get; set; } = 1;
        //public int RewardLevel { get; set; } = 0;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}