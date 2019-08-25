using Harmony12;
using Pathea.NpcRepositoryNs;
using Pathea.ChildrenNs;
using Pathea.FavorSystemNs;
using UnityEngine;
using System;

namespace MarriageMod
{
    public static partial class Main
    {
        
        // make everybody dateable

        [HarmonyPatch(typeof(FavorRelationshipData), "GetRefData", new Type[] { typeof(int) })]
        static class FavorRelationshipData_GetRefData_Patch
        {
            static bool Prefix(int id, ref FavorRelationshipData __result)
            {
                if (!enabled || ChildrenModule.IsChild(id))
                    return true;

                if (FavorRelationshipData.refDataDic.ContainsKey(id))
                {
                    FavorRelationshipData frd = FavorRelationshipData.refDataDic[id];

                    if(frd.canPlay)
                        frd.canExpress = true;
                    __result = frd;
                    return false;
                }
                __result = null;
                return false;

            }
        }
    }
}