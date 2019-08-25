using Harmony12;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using Pathea.Behavior;
using System;

namespace MarriageMod
{
    public static partial class Main
    {

        //remove negative favor during engagement from jealousy

        [HarmonyPatch(typeof(EGGainFavor), "OnUpdate", new Type[] { })]
        static class EGGainFavor_OnUpdate_Patch
        {
            static bool Prefix(ref TaskStatus __result, SharedInt ___favorValue)
            {
                if (!Main.enabled)
                    return true;

                if (___favorValue.Value < 0)
                {
                    Dbgl("EGGainFavor_OnUpdate_Patch");
                    __result = TaskStatus.Failure;
                    return false;
                }
                return true;
            }
        }
    }
}