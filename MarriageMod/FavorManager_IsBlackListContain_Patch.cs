using Harmony12;
using Pathea.FavorSystemNs;

namespace MarriageMod
{
    public static partial class Main
    {
        
        // make everybody dateable

        [HarmonyPatch(typeof(FavorManager), nameof(FavorManager.IsBlackListContain))]
        static class FavorManager_IsBlackListContain_Patch
        {
            static bool Prefix( ref bool __result)
            {
                if (!enabled)
                    return true;
                __result = false;
                return false;
            }
        }
    }
}