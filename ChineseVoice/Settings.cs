using UnityModManagerNet;

namespace ChineseVoice
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool AlwaysChinese { get; internal set; } = false;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}