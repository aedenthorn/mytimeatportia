using Harmony12;
using Pathea.Behavior;
using System;

namespace MarriageMod
{
    public static partial class Main
    {
        // removing ending of date due to jealousy

        [HarmonyPatch(typeof(EGStopEngagement), "OnStart", new Type[] { })]
        static class EGStopEngagement_OnStart_Patch
        {
            static bool Prefix()
            {
                if (!Main.enabled)
                    return true;
                Dbgl("EGStopEngagement_OnStart_Patch");

                return false;
            }
        }
    }
}