using Harmony12;
using Pathea.HomeNs;
using System;

namespace InstantActions
{
    public partial class Main
    {
        // instant petting

        [HarmonyPatch(typeof(FarmAnimalCtr), "TouchAnimal", new Type[] { typeof(bool),typeof(bool) })]
        static class FarmAnimalCtr_TouchAnimal_Patch
        {
            static void Prefix(ref bool playerAnim, ref bool hideWeapon)
            {
                if (!Main.enabled || !settings.InstantFarmPet)
                    return;
                playerAnim = false;
                hideWeapon = false;
            }
        }

    }
}
