using Harmony12;
using System;

namespace MarriageMod
{
    public static partial class Main
    {
        [HarmonyPatch(typeof(Pathea.Player), "CanInteractiveJealous", new Type[] { })]
        static class Pathea_Player_CanInteractiveJealous_Patch
        {
            static bool Prefix(ref bool __result)
            {
                if (!Main.enabled)
                    return true;
                __result = false;
                return false;
            }
        }
    }
}