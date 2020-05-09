using Harmony12;
using Pathea.NpcInstanceNs;
using System;
using System.Reflection;

namespace MarriageMod
{
    public static partial class Main
    {
        // stop any stray jealous checks or sets

        [HarmonyPatch(typeof(NpcRelationModule.RelationFlagTime), "Dispose")]
        static class NpcRelationModule_RelationFlagTime_Dispose_Patch
        {
            static bool Prefix(NpcRelationModule.RelationFlagTime __instance)
            {
                if (!Main.enabled)
                    return true;
                if ((__instance.flag & 1) != 0)
                {
                    Dbgl(__instance.flag + " disposed");
                    //typeof(NpcRelationModule.RelationFlag).GetMethod("Dispose", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { });
                    return false;
                }
                return true;
            }
        }
    }
}