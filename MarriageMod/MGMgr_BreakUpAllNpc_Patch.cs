using Harmony12;
using System;

namespace MarriageMod
{
    public static partial class Main
    {
        // stops breakups on marriage

        [HarmonyPatch(typeof(Pathea.MG.MGMgr), "BreakUpAllNpc", new Type[] { })]
        static class MGMgr_BreakUpAllNpc_Patch
        {
            static bool Prefix()
            {
                if (!enabled)
                    return true;

                return false;
            }
        }
    }
}