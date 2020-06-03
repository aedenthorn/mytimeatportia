using UnityModManagerNet;

namespace SaveAnyTime
{
    public class Settings : UnityModManager.ModSettings
    {
        public int maxAutoSaves = 1;
        public int saveInterval = 0;
        public bool saveOnSceneChange = false;

        public string QuickSaveKey { get; set; } = "f6";
        public string QuickLoadKey { get; set; } = "f7";

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}