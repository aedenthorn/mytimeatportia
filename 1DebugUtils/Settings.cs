using UnityModManagerNet;

namespace FieldOfView
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool WriteNativeLog { get; set; } = true;
        public bool StackTrace { get; set; } = true;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
} 