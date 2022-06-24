using UnityModManagerNet;

namespace YourTime
{
    public class Settings : UnityModManager.ModSettings
    {
        public float TimeScaleModifier { get; set; } = 0.5f;
        public string SpeedModKey { get; set; } = "left shift";
        public string DayModKey { get; set; } = "left shift";
        public string MonthModKey { get; set; } = "left ctrl";
        public string YearModKey { get; set; } = "left alt";
        public string StopTimeKey { get; set; } = "\\";
        public string SubtractTimeKey { get; set; } = "-";
        public string AdvanceTimeKey { get; set; } = "=";
        public string SlowTimeKey { get; set; } = "[";
        public string SpeedTimeKey { get; set; } = "]";

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}