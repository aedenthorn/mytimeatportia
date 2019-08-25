using Harmony12;
using Pathea.Behavior;
using System;

namespace MarriageMod
{
    public static partial class Main
    {
        // Remove Jealousy

        [HarmonyPatch(typeof(Jealous), "OnStart", new Type[] { })]
        static class Jealous_OnStart_Patch
        {
            static bool Prefix()
            {
                if (!Main.enabled)
                    return true;
                Dbgl("Jealous_OnStart_Patch");
                return false;
            }
        }
    }
}