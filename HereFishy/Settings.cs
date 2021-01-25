using UnityModManagerNet;

namespace HereFishy
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool PlayHereFishy { get; set; } = true;
        public bool PlayWee { get; set; } = true;
        public float HereFishyVolume { get; set; } = 0.9f;
        public float WeeVolume { get; set; } = 0.9f;
        public float JumpHeight { get; set; } = 3f;
        public float JumpSpeed { get; set; } = 0.3f;
        public float Volume { get; set; } = 1f;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
} 