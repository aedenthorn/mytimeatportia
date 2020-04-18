using UnityModManagerNet;

namespace UnlimitedCivicCorpsMissions
{
    public class Settings : UnityModManager.ModSettings
    {
        public int MaxMissions { get; set; } = 1000;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}