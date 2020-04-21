using UnityModManagerNet;

namespace MusicBox
{
    public class Settings : UnityModManager.ModSettings
    {
        public float MusicVolume { get; set; } = 2f;
        public float MinDistance { get; set; } = 10f;
        public float MaxDistance { get; set; } = 20f;
        public bool LoopAudio { get; set; } = true;
        public bool PlayAll { get; set; } = true;
        public bool ShuffleOrder { get; set; } = true;
        public bool SilenceAlarm { get; set; } = false;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}