using Harmony12;
using Pathea.ChildrenNs;
using Pathea.MG;
using Pathea.ModuleNs;
using System;
using UnityEngine;

namespace MarriageMod
{
    public static partial class Main
    {
        // set children's parent to settings spouse

        [HarmonyPatch(typeof(ChildrenModule), "CanShowWantChildTips", new Type[] { typeof(int) })]
        static class ChildrenModule_CanShowWantChildTips_Patch
        {
            static void Prefix(ref int ___marriageID)
            {
                if (!enabled)
                    return;
                ___marriageID = GetCurrentSpouse();
            }
        }
    }
}
