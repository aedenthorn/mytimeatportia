using UnityModManagerNet;

namespace MonsterVacuum
{
    public class Settings : UnityModManager.ModSettings
    {
        public int VacuumRadius = 5;
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}