using UnityEngine;
using Harmony12;
using Pathea.Behavior;
using System;

namespace MarriageMod
{
    public static partial class Main
    {
        // remove jealousy on interactions

        [HarmonyPatch(typeof(ITJealous), "OnStart", new Type[] { })]
        static class ITJealous_OnStart_Patch
        {
            static bool Prefix()
            {
                if (!Main.enabled)
                    return true;
                Dbgl("ITJealous_OnStart_Patch");
                return false;
            }
        }
    }
}