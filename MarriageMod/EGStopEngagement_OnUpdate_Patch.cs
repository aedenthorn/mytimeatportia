using Harmony12;
using BehaviorDesigner.Runtime.Tasks;
using Pathea.Behavior;
using System;

namespace MarriageMod
{
    public static partial class Main
    {

        // prevents waiting for engagement to end due to jealousy

        [HarmonyPatch(typeof(EGStopEngagement), "OnUpdate", new Type[] {   })]
        static class EGStopEngagement_OnUpdate_Patch
        {
            static bool Prefix(ref TaskStatus __result)
            {
                if (!Main.enabled)
                    return true;

                __result = TaskStatus.Failure;
                return false;
            }
        }
    }
}