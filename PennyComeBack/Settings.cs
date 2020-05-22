using UnityModManagerNet;

namespace PennyComeBack
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool ReplaceMusic { get; internal set; } = false;
        public bool UseEnglish { get; internal set; } = true;
        public bool UseChinese { get; internal set; } = false;
        public float SpatialBlend { get; internal set; } = 1.0f;
        public float MusicVolume { get; internal set; } = 1f;
        public int MusicDistance { get; internal set; } = 10000;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}