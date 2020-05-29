using Harmony12;
using Pathea.MG;
using Pathea.FavorSystemNs;
using System;
using Pathea.ActorNs;
using System.Collections.Generic;
using UnityEngine;
using Pathea.NpcRepositoryNs;

namespace MarriageMod
{
    public static partial class Main
    {
        // tell all spouses they're married

        [HarmonyPatch(typeof(MGMgr), "IsMarriage", new Type[] {})]
        static class MGMgr_IsMarriage2_Patch
        {
            static bool Prefix(ref bool __result, ref MGState ___mState, ref Actor ___mActor, ref int ___mNpcID)
            {
                if (!enabled)
                    return true;

                __result = false;

                int spouseid = GetCurrentSpouse();
                Dbgl("XYZ current spouse is " + spouseid);

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
                foreach (FavorObject f in fList)
                {
                    if (f.RelationshipType == FavorRelationshipType.Couple)
                    {
                        __result = true;
                        if (___mState == MGState.Single)
                            ___mState = MGState.Marriage;

                        if(spouseid == f.ID)
                        {
                            ___mActor = ActorMgr.Self.Get(f.ID);
                            ___mNpcID = f.ID;
                        }

                    }
                }

                return false;
            }
        }
    }
}