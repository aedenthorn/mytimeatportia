using UnityModManagerNet;

namespace FactoryStorageSize
{
    public class Settings : UnityModManager.ModSettings
    {
        public int FactoryStorageSize { get; set; } = 300;
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
} 