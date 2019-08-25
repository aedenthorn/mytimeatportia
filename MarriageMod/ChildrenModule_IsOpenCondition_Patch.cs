using Harmony12;
using Pathea.MG;
using Pathea.FavorSystemNs;
using Pathea;
using Pathea.ChildrenNs;
using Hont;
using Pathea.HomeNs;
using UnityEngine;
using Pathea.ModuleNs;
using System;

namespace MarriageMod
{
    public static partial class Main
    {
        // set married days required before asking for kids

        [HarmonyPatch(typeof(ChildrenModule), "IsOpenCondition", new Type[] {  })]
        static class ChildrenModule_IsOpenCondition_Patch
        {
            static bool Prefix(ref bool __result, ref int ___marriageID, ref bool ___isMiscarry, ref GameDateTime ___lastMiscarryTime)
            {
                if (!enabled)
                    return true;

                if (!ChildrenModule.ModuleOpen)
                {
                    __result = false;
                    return false;
                }
                ___marriageID = GetCurrentSpouse();
                if (___marriageID < 1)
                {
                    __result = false;
                    return false;
                }

                bool attain = FavorRelationshipUtil.GetRelationShip(___marriageID) == ChildConfig.Self.RelationShip;
                bool home = Module<HomeModule>.Self.HomeLevel >= ChildConfig.Self.HomeLevel;
                bool free = Singleton<BedUnitMgr>.Self.HaveChildsFreePos();

                __result = attain && home && free;
                return false;
            }
        }
    }
}