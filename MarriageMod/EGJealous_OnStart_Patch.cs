using Harmony12;
using Pathea.Behavior;
using System;

namespace MarriageMod
{
    public static partial class Main
    {
        // remove jealousy during dates

        [HarmonyPatch(typeof(EGJealous), "OnStart", new Type[] { })]
        static class EGJealous_OnStart_Patch
        {
            static bool Prefix()
            {
                if (!Main.enabled)
                    return true;
                Dbgl("EGJealous_OnStart_Patch");
                return false;
            }
        }
    }
}