using Harmony12;
using BehaviorDesigner.Runtime.Tasks;
using Pathea.Behavior;
using System;

namespace MarriageMod
{
    public static partial class Main
    {
        // remove negative favour for jealosy during dates

        [HarmonyPatch(typeof(EGChangeRelationFlag), "OnUpdate", new Type[] { })]
        static class EGChangeRelationFlag_OnUpdate_Patch
        {
            static bool Prefix(ref TaskStatus __result)
            {
                if (!Main.enabled)
                    return true;
                Dbgl("EGChangeRelationFlag_OnUpdate_Patch");
                __result = TaskStatus.Failure;
                return false;
            }
        }
    }
}