using UnityModManagerNet;

namespace Regeneration
{
    public class Settings : UnityModManager.ModSettings
    {
        public float HealthRegen { get; set; } = 1f;
        public float StaminaRegen { get; set; } = 1f;
        public bool RegenWhenStopped { get; set; } = true;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
} 