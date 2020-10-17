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
                try {
                    typeof(FavorManager).GetMethod("RemoveFromBlackList").Invoke(Module<FavorManager>.Self, new object[] { 4000097 });
                    Dbgl("Making Higgins datable (1)");
                }
                catch { }
                try {
                    typeof(FavorManager).GetMethod("RemoveToBlackList").Invoke(Module<FavorManager>.Self, new object[] { 4000097 });
                    Dbgl("Making Higgins datable (2)");
                }
                catch { }
            }
        }
    }
}