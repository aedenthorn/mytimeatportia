using UnityModManagerNet;

namespace InstantLoot
{
    public class Settings : UnityModManager.ModSettings
    {
        public float DistanceMult { get; set; } = 1f;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}