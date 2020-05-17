using System.Collections.Generic;
using UnityModManagerNet;

namespace WeaponMod
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool includeSpecialWeapons = false;
        public int specialWeaponStore = 8;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}