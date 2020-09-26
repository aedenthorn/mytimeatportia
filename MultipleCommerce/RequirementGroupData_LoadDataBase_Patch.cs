using Harmony12;
using Hont.ExMethod.Collection;
using Pathea.Missions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultipleCommerce
{
    public static partial class Main
    {
        [HarmonyPatch(typeof(RequirementGroupData), "LoadDataBase", new Type[] { })]
        static class RequirementGroupData_LoadDataBase_Patch
        {
            static void Postfix()
            {
                if (!enabled)
                    return;

                if (settings.AddFishSpecialOrders)
                {
                    int[] fi = new int[FishInts.Length];
                    for (int i = 0; i < FishInts.Length; i++)
                    {
                        fi[i] = DIVERSE_ID_COUNTER++;
                    }
                    RequirementGroupData.refDataDic[FISH_GROUP_ID] = new RequirementGroupData(FISH_GROUP_ID, fi, 0);
                }
                if (settings.MoreDiverseOrders)
                {
                    IEnumerable<int> keys = RequirementGroupData.refDataDic.Keys;
                    foreach(int key in keys)
                    {
                        var rqd = RequirementGroupData.refDataDic[key];
                        List<int> ids = new List<int>(rqd.requirementIdAry);
                        foreach(int j in rqd.requirementIdAry)
                        {
                            var rd = RequirementData.refDataDic[j];
                            int level = rd.level;
                            while (level < 6)
                            {
                                ids.Add(DIVERSE_ID_COUNTER);
                                level++;
                                int mult = level - rd.level;
                                RequirementData.refDataDic[DIVERSE_ID_COUNTER] = new RequirementData(DIVERSE_ID_COUNTER, rd.itemId, new DoubleInt(rd.itemCount.id0 * mult, rd.itemCount.id1 * mult), new DoubleInt(rd.rewardGold.id0 * mult, rd.rewardGold.id1 * mult), new DoubleInt(rd.rewardExp.id0 * mult, rd.rewardExp.id1 * mult), new DoubleInt(rd.rewardLikability.id0 * mult, rd.rewardLikability.id1 * mult), new DoubleInt(rd.rewardReputation.id0 * mult, rd.rewardReputation.id1 * mult), level, level, rd.weight, rd.deadLine, rd.seasonReq, rd.weatherReq);
                                DIVERSE_ID_COUNTER++;
                            }
                        }
                        RequirementGroupData.refDataDic[key].requirementIdAry = ids.ToArray();
                    }
                }
            }
        }
    }
}