using Harmony12;
using Pathea.MG;
using Pathea.FavorSystemNs;
using Pathea;
using Pathea.HomeNs;
using Pathea.ChildrenNs;
using System.Collections.Generic;
using Pathea.ActorNs;
using Pathea.ModuleNs;
using System;

namespace MarriageMod
{
    public static partial class Main
    {
        // add all spouses to bed selection

        [HarmonyPatch(typeof(BedUnitMgr), "FreshList", new Type[] {  })]
        static class BedUnitMgr_FreshList_Patch
        {
            
            static bool Prefix(ref Dictionary<int, BedType> ___userTypeMap, ref List<int> ___userActors, List<BedUnit> ___units, Dictionary<int,BedUnit> __state, List<int> ___desUserActors)
            {
                if (!enabled)
                    return true;
                List<int> list = new List<int>();
                ___userTypeMap.Clear();
                list.Add(Module<Player>.Self.actor.InstanceId);
                ___userTypeMap.Add(Module<Player>.Self.actor.InstanceId, BedType.Adult);

                List<FavorObject> fList = new List<FavorObject>(FavorManager.Self.GetAllShowFavorObjects());
                foreach (FavorObject f in fList)
                {
                    if (f.RelationshipType == FavorRelationshipType.Couple)
                    {
                        ___userTypeMap.Add(f.ID, BedType.Adult); // add new spouse to new bed
                        list.Add(f.ID);

                        Actor a = ActorMgr.Self.Get(f.ID);
                        a.ReleaseStop(MGConst.BehaviourStop);
                        a.SetSpouse(Module<Player>.Self.actor);

                    }
                }

                int[] children = ChildrenModule.Self.GetChildren();
                for (int i = 0; i < children.Length; i++)
                {
                    if (children[i] > 0)
                    {
                        list.Add(children[i]);
                        ___userTypeMap.Add(children[i], BedType.Child);
                    }
                }

                if (___desUserActors != null)
                {
                    for (int m = 0; m < list.Count; m++)
                    {
                        ___desUserActors.Remove(list[m]);
                    }
                    for (int n = 0; n < ___desUserActors.Count; n++)
                    {
                        for (int num = 0; num < ___units.Count; num++)
                        {
                            int bindIndex2 = ___units[num].GetBindIndex(___desUserActors[n]);
                            if (bindIndex2 >= 0)
                            {
                                ___units[num].Unbind(bindIndex2);
                            }
                        }
                    }
                    ___desUserActors = null;
                }

                ___userActors = list;
                return false;
            }
        }
    }
}