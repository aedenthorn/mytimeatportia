using UnityModManagerNet;

namespace ProductionTimeMod
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool RealTime { get; set; } = true;
        public float SpeedMult { get; set; } = 1;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}