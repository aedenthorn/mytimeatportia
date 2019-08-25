using Harmony12;
using Pathea.MG;
using Pathea.FavorSystemNs;
using Pathea.HomeNs;
using Pathea.ActorNs;
using Pathea;
using Pathea.ModuleNs;
using System;

namespace MarriageMod
{
    public static partial class Main
    {
        // set last bedded spouse as official spouse (also start resetting other spouses to come to beds)

        [HarmonyPatch(typeof(BedUnitMgr), "Bind", new Type[]{typeof(int),typeof(BedUnit),typeof(int)})]
        static class BedUnitMgr_Bind_Patch
        {

            static void Prefix(int userId)
            {
                if (!enabled)
                    return;

                if (userId < 0)
                    return;

                FavorObject f = FavorManager.Self.GetFavorObject(userId);
                if (f != null && f.RelationshipType == FavorRelationshipType.Couple) {
                    Actor a = ActorMgr.Self.Get(f.ID);
                    a.ReleaseStop(MGConst.BehaviourStop);
                    a.SetSpouse(Module<Player>.Self.actor);
                }
            }
        }
    }
}