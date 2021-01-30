using UnityModManagerNet;

namespace EnhancedRomanceChance
{
    public class Settings : UnityModManager.ModSettings
    {
        public int ConfessPercentChanceIncrease { get; set; } = 10;
        public int ConfessPercentChanceIncreasePerConfession { get; set; } = 10;
        public int ProposePercentChanceIncrease { get; set; } = 10;
        public int ProposePercentChanceIncreasePerProposal { get; set; } = 10;
        public string RomanceModFolderName { get; set; } = "EnhancedRomanceChance";
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}