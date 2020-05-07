using UnityModManagerNet;

namespace StorageAnywhere
{
    public class Settings : UnityModManager.ModSettings
    {
        public string ItemBarSwitchKey { get; set; } = "tab";
        public string OpenStorageKey { get; set; } = "b";
        public bool RememberLastStorageUnit { get; set; } = true;
        public string OpenFactoryKey { get; set; } = "n";
        public string PrevStorageKey { get; set; } = "a";
        public string NextStorageKey { get; set; } = "d";

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}