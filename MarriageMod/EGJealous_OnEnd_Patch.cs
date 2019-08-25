using Harmony12;
using Pathea.Behavior;
using System;

namespace MarriageMod
{
    public static partial class Main
    {
        [HarmonyPatch(typeof(EGJealous), "OnEnd", new Type[] { })]
        static class EGJealous_OnEnd_Patch
        {
            static bool Prefix()
            {
                if (!Main.enabled)
                    return true;
                Dbgl("EGJealous_OnEnd_Patch");
                return false;
            }
        }
    }
}