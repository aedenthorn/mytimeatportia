using UnityModManagerNet;

namespace MoreDisplayedMissions
{
    public class Settings : UnityModManager.ModSettings
    {
        public int MaxMissions = 5;
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
} 