using Harmony12;
using Pathea.NpcInstanceNs;
using System;

namespace MarriageMod
{
    public static partial class Main
    {
        [HarmonyPatch(typeof(NpcRelationModule), "AddFlag", new Type[] { typeof(int), typeof(int), typeof(int), typeof(NpcRelationModule.RelationFlagFunc.RelationDelete) })]
        static class NpcRelationModule_AddFlag_Patch
        {
            static bool Prefix(int flag, ref int __result)
            {
                if (!Main.enabled)
                    return true;
                if(flag == (int)NpcRelationModule.RelationMask.Jealous)
                {
                    __result = -1;
                    return false;
                }
                return true;
            }
        }
    }
}