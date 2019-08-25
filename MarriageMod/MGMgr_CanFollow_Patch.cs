using Harmony12;
using Pathea.MG;
using System;

namespace MarriageMod
{
    public static partial class Main
    {
        [HarmonyPatch(typeof(MGMgr), "CanFollow", new Type[] { })]
        static class MGMgr_CanFollow_Patch
        {
            static bool Prefix(ref bool __result)
            {
                if (!enabled)
                    return true;
                __result = true;
                return false;
            }
        }
    }
}