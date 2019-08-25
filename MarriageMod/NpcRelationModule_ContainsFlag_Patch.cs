using Harmony12;
using Pathea.NpcInstanceNs;
using System;

namespace MarriageMod
{
    public static partial class Main
    {
        // stop any stray jealous checks or sets

        [HarmonyPatch(typeof(NpcRelationModule), "ContainsFlag", new Type[] { typeof(int), typeof(int), typeof(int) })]
        static class NpcRelationModule_ContainsFlag_Patch
        {
            static bool Prefix(int flag, ref bool __result)
            {
                if (!Main.enabled)
                    return true;
                if(flag == (int)NpcRelationModule.RelationMask.Jealous)
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }
    }
}