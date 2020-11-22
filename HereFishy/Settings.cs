using UnityModManagerNet;

namespace HereFishy
{
    public class Settings : UnityModManager.ModSettings
    {
        internal bool PlayHereFishy { get; set; } = true;
        internal bool PlayWee { get; set; } = true;

        public float JumpHeight { get; set; } = 3f;
        public float JumpSpeed { get; set; } = 0.3f;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
} 