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
                if (typeof(FavorManager).GetMethod("RemoveFromBlackList") != null)
                {
                    typeof(FavorManager).GetMethod("RemoveFromBlackList").Invoke(Module<FavorManager>.Self, new object[] { 4000097 }); 
                }
                else if (typeof(FavorManager).GetMethod("RemoveToBlackList") != null)
                {
                    typeof(FavorManager).GetMethod("RemoveToBlackList").Invoke(Module<FavorManager>.Self, new object[] { 4000097 }); 
                }
            }
        }
    }
}