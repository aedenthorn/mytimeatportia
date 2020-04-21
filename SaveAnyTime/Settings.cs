using UnityModManagerNet;

namespace SaveAnyTime
{
    public class Settings : UnityModManager.ModSettings
    {
        public string QuickSaveKey { get; set; } = "f6";

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}