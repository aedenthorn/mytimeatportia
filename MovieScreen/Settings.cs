using UnityModManagerNet;

namespace MovieScreen
{
    public class Settings : UnityModManager.ModSettings
    {
        public float Spatiality { get; set; } = 0.1f;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
} 