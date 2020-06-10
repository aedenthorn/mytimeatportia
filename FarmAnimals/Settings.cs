using UnityModManagerNet;

namespace FarmAnimals
{
    public class Settings : UnityModManager.ModSettings
    {
        public float AnimalGrowthSpeed { get; set; } = 1.0f;
        public float MaxAnimalGrowthPercent { get; set; } = 200.0f;
        public float BasePregnancyChance { get; set; } = 10.0f;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}