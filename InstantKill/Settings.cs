using UnityModManagerNet;

namespace InstantKill
{
    public class Settings : UnityModManager.ModSettings
    {
        public float KillDistance { get; set; } = 5f;
        public int KillIntervalSec { get; set; } = 1;
        public string KillToggleButtom { get; set; } = "backspace";
        public bool KillEnable { get; set; } = true;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
} 