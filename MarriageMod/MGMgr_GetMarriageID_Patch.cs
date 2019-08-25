using Harmony12;
using Pathea.MG;
using Pathea.FavorSystemNs;
using System;
using Pathea.ActorNs;
using UnityEngine;
using Pathea.NpcRepositoryNs;
using Pathea.ModuleNs;

namespace MarriageMod
{
    public static partial class Main
    {
        // set spouse to settings spouse

        [HarmonyPatch(typeof(MGMgr), "GetMarriageID", new Type[] { })]
        static class MGMgr_GetMarriageID_Patch
        {
            static bool Prefix(ref int ___mNpcID, ref Actor ___mActor, ref int __result, ref MGState ___mState)
            {
                if (!enabled)
                    return true;
                __result = 0;
                if (Module<MGMgr>.Self.IsPropose())
                {
                    __result = ___mNpcID;
                    Debug.LogError("XYZ Proposing! Current fiance is " + Module<NpcRepository>.Self.GetNpcName(___mNpcID) + " " + ___mNpcID);
                }
                else
                {
                    int npcId = GetCurrentSpouse();
                    ___mNpcID = npcId;
                    ___mActor = ActorMgr.Self.Get(npcId);
                    __result = npcId;
                    if (___mState == MGState.Marriage && npcId == 0)
                    {
                        Debug.LogError("XYZ Single! No spouse.");
                        ___mState = MGState.Single;
                    }
                    else if (___mState == MGState.Single && npcId != 0)
                    {
                        ___mState = MGState.Marriage;
                        Debug.LogError("XYZ Married! Current spouse is " + Module<NpcRepository>.Self.GetNpcName(___mNpcID) + " " + ___mNpcID);
                    }
                }
                return false;
            }
        }
    }
}