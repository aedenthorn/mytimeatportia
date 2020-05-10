using UnityModManagerNet;

namespace CustomizePlayer
{
    public class Settings : UnityModManager.ModSettings
    {
        public string OpenCustomizeKey { get; set; } = "y";

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}