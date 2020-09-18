using System.Collections.Generic;
using UnityModManagerNet;

namespace WeaponMod
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool includeSpecialWeapons = true;
        public int specialWeaponStore = 8;
        public float waterSwordChance = 0.5f;
        public float purpleHazeChance = 0.5f; 
        public float InflateableHammerChance = 0.5f;
        public float DevDaggerChance = 0.5f;
        public float unknownWeaponChance = 0.1f;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}