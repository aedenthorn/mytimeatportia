using Harmony12;
using Pathea.FavorSystemNs;
using System;
using Pathea.ModuleNs;

namespace MarriageMod
{
    public static partial class Main
    {
        // make 4000097 dateable

        [HarmonyPatch(typeof(FavorManager), "InitData", new Type[] { })]
        static class FavorManager_InitData_Patch
        {
            static void Postfix()
            {
                Module<FavorManager>.Self.RemoveToBlackList(4000097);
            }
        }
    }
}