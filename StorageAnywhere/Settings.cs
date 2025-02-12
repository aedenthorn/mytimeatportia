using UnityModManagerNet;

namespace StorageAnywhere
{
    public class Settings : UnityModManager.ModSettings
    {
        public string ItemBarSwitchKey { get; set; } = "tab";
        public string OpenStorageKey { get; set; } = "b";
        public bool RememberLastStorageUnit { get; set; } = true;
        public string OpenFactoryModKey { get; set; } = "left ctrl";
        public string OpenFactoryProductModKey { get; set; } = "left shift";
        public string PrevStorageKey { get; set; } = "w";
        public string NextStorageKey { get; set; } = "s";
        public string PrevPageKey { get; set; } = "a";
        public string NextPageKey { get; set; } = "d";
        public int FactoryStorageSize { get; set; } = 300;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}