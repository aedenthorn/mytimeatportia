using UnityModManagerNet;

namespace StorageSize
{
    public class Settings : UnityModManager.ModSettings
    {
        public int WoodenStorageSize { get; set; } = 30;
        public int MetalStorageSize { get; set; } = 60;
        public int SafetyBoxSize { get; set; } = 90;
        public bool UpdateExistingStorages { get; set; } = false;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}