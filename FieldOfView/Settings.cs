using UnityModManagerNet;

namespace FieldOfView
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool UseScrollWheel { get; set; } = true;
        public float FieldOfView { get; set; } = 45;
        public float IncrementNormal { get; set; } = 1;
        public float IncrementFast { get; set; } = 5;
        public string KeyDecrease { get; set; } = "";
        public string KeyIncrease { get; set; } = "";
        public string ModKeyNormal { get; set; } = "left ctrl";
        public string ModKeyFast { get; set; } = "left alt";

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
} 