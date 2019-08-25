using Harmony12;
using System;

namespace MarriageMod
{
    public static partial class Main
    {
        // removing jealousy during intimacy

        [HarmonyPatch(typeof(Pathea.Player), "InteractiveJealous", new Type[] { })]
        static class Pathea_Player_InteractiveJealous_Patch
        {
            static bool Prefix()
            {
                if (!Main.enabled)
                    return true;
                Dbgl("Pathea_Player_InteractiveJealous_Patch");

                return false;
            }
        }
    }
}