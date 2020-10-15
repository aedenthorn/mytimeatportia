using UnityModManagerNet;

namespace MoreSkillPoints
{
    public class Settings : UnityModManager.ModSettings
    {
        public int ExtraPoints = 1;
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
} 