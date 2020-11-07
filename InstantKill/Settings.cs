using UnityModManagerNet;

namespace InstantKill
{
    public class Settings : UnityModManager.ModSettings
    {
        public float KillDistance { get; set; } = 20f;
        public string KillButton { get; set; } = "backspace";

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
} 