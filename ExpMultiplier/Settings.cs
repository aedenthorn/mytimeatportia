using UnityModManagerNet;

namespace ExpMultiplier
{
    public class Settings : UnityModManager.ModSettings
    {
        public float ExpMult { get; set; } = 1f;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}