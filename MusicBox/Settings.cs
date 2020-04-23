using UnityEngine;
using UnityModManagerNet;

namespace MusicBox
{
    public class Settings : UnityModManager.ModSettings
    {
        public float MusicVolume { get; set; } = 1f;
        public float MinDistance { get; set; } = 10f;
        public float MaxDistance { get; set; } = 30f;
        public float Spatiality { get; set; } = 0.7f;
        public bool LoopAudio { get; set; } = true;
        public bool PlayAll { get; set; } = true;
        public bool ShuffleOrder { get; set; } = true;
        public bool SilenceAlarm { get; set; } = false;
        public bool UseUncompressedWAVFiles { get; set; } = false;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}