using Harmony12;
using BehaviorDesigner.Runtime.Tasks;
using Pathea.Behavior;
using System;

namespace MarriageMod
{
    public static partial class Main
    {
        [HarmonyPatch(typeof(Jealous), "OnUpdate", new Type[] { })]
        static class Jealous_OnUpdate_Patch
        {
            static bool Prefix(ref TaskStatus __result)
            {
                if (!Main.enabled)
                    return true;
                Dbgl("Jealous_OnUpdate_Patch");
                __result = TaskStatus.Failure;
                return false;
            }
        }
    }
}