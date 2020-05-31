using UnityModManagerNet;
namespace PlantMod
{
    public class Settings : UnityModManager.ModSettings
    {
        public float nutrientConsumeMult = 1f;
        public float plantGrowMult = 1f;


        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}