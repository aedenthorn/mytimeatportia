using UnityModManagerNet;

namespace HereFishy
{
    public class Settings : UnityModManager.ModSettings
    {
        public float SlotCostMult { get; set; } = 1;
        public float SlotRewardMult { get; set; } = 1;
        public float SlotSpeedMult { get; set; } = 1;
        public float BalloonBulletCountMult { get; set; } = 1;
        public float BalloonScoreMult { get; set; } = 1;
        public float BalloonScaleMult { get; set; } = 1;
        public float BalloonRewardMult { get; set; } = 1;
        public float DartSpeedMult { get; set; } = 1;
        public float DartRewardMult { get; set; } = 1;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
} 