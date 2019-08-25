using Harmony12;
using Pathea.MG;
using System;

namespace MarriageMod
{
    public static partial class Main
    {
        // allowing marriage while married

        [HarmonyPatch(typeof(Pathea.MG.MGMgr), "TryPropose", new Type[] { typeof(int) })]
        static class MGMgr_TryPropose_Patch
        {
            static void Prefix(ref int ___mNpcID, ref MGState ___mState)
            {
                if (!enabled)
                    return;

                ___mNpcID = 0;
                ___mState = MGState.Single;
            }
        }
    }
}