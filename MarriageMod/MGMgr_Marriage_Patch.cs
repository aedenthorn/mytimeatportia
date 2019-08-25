using Harmony12;
using Pathea.MG;
using System;

namespace MarriageMod
{
    public static partial class Main
    {
         
        // set new spouse when getting married

        [HarmonyPatch(typeof(Pathea.MG.MGMgr), "Marriage", new Type[] { })]
        static class MGMgr_Marriage_Patch
        {
            static void Prefix(ref int ___mNpcID, ref MGState ___mState)
            {
                if (!enabled)
                    return;

                if (___mState == MGState.Propose)
                {
                    settings.CurrentSpouse = ___mNpcID;
                }
            }
        }
    }
}