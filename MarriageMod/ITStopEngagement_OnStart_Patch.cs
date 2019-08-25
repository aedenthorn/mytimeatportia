using Harmony12;
using Pathea.Behavior;
using System;

namespace MarriageMod
{
    public static partial class Main
    {
        [HarmonyPatch(typeof(ITStopEngagement), "OnStart", new Type[] { })]
        static class ITStopEngagement_OnStart_Patch
        {
            static bool Prefix()
            {
                if (!Main.enabled)
                    return true;
                Dbgl("ITStopEngagement_OnStart_Patch");

                return false;
            }
        }
    }
}