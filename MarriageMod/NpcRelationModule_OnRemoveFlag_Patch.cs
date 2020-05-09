using Harmony12;
using Pathea.NpcInstanceNs;
using System;

namespace MarriageMod
{
    public static partial class Main
    {
        // stop any stray jealous checks or sets

        [HarmonyPatch(typeof(NpcRelationModule), "OnRemoveFlag", new Type[] { typeof(int), typeof(int), typeof(int) })]
        static class NpcRelationModule_OnRemoveFlag_Patch
        {
            static bool Prefix(int flag)
            {
                if (!Main.enabled)
                    return true;
                if ((flag & 1) != 0)
                {
                    Dbgl(flag + " removed");
                    return false;
                }
                return true;
            }
        }
    }
}