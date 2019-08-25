using Harmony12;
using Pathea.Behavior;
using System;

namespace MarriageMod
{
    public static partial class Main
    {
        [HarmonyPatch(typeof(Jealous), "OnEnd", new Type[] { })]
        static class Jealous_OnEnd_Patch
        {
            static bool Prefix()
            {
                if (!Main.enabled)
                    return true;
                Dbgl("Jealous_OnEnd_Patch");
                return false;
            }
        }
    }
}