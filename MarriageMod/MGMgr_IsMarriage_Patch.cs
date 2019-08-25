using Harmony12;
using Pathea.MG;
using Pathea.FavorSystemNs;
using System;
using Pathea;
using UnityEngine;
using Pathea.NpcRepositoryNs;
using Pathea.ActorNs;
using System.Collections.Generic;
using Pathea.ModuleNs;

namespace MarriageMod
{
    public static partial class Main
    {
        // tell all spouses they're married

        [HarmonyPatch(typeof(MGMgr), "IsMarriage", new Type[] { typeof(int) })]
        static class MGMgr_IsMarriage_Patch
        {
            static bool Prefix(int npcId, ref bool __result, ref MGState ___mState, ref Actor ___mActor, ref int ___mNpcID)
            {
                if (!enabled)
                    return true;
                //Debug.Log("XYZ Checking if "+Module<NpcRepository>.Self.GetNpcName(npcId)+" "+npcId+" is Spouse");

                FavorObject f = FavorManager.Self.GetFavorObject(npcId);
                __result = (f.RelationshipType == FavorRelationshipType.Couple);

                //Debug.Log("XYZ " + Module<NpcRepository>.Self.GetNpcName(npcId) + " " + npcId + " is"+(__result?" ":" not ")+"Spouse");

                int spouseid = GetCurrentSpouse();
                if (spouseid == 0)
                {
                    if (___mState != MGState.Propose)
                    {
                        ___mState = MGState.Single;
                        ___mActor = null;
                        ___mNpcID = 0;
                    }
                    return false;
                }

                List<FavorObject> fList = new List<FavorObject>(FavorManager.Self.GetAllShowFavorObjects());
                foreach (FavorObject fo in fList)
                {
                    if (fo.RelationshipType == FavorRelationshipType.Couple)
                    {
                        if (___mState == MGState.Single)
                            ___mState = MGState.Marriage;

                        if (spouseid == fo.ID && ___mState != MGState.Propose)
                        {
                            ___mActor = ActorMgr.Self.Get(fo.ID);
                            ___mNpcID = fo.ID;
                        }

                    }
                }

                return false;
            }
        }
    }
}