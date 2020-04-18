using Harmony12;
using Pathea.Missions;
using System;

namespace MultipleCommerce
{
    public static partial class Main
    {
        [HarmonyPatch(typeof(RequirementGroupData), "LoadDataBase", new Type[] { })]
        static class RequirementGroupData_LoadDataBase_Patch
        {
            static void Postfix()
            {
                if (!enabled || !settings.AddFishSpecialOrders)
                    return;
                RequirementGroupData r = RequirementGroupData.refDataDic[FISH_GROUP_ID];
                int[] fi = new int[FishInts.Length];
                int index = FISH_BASE_ID;
                for (int i = 0; i < FishInts.Length; i++)
                {
                    fi[i] = index++;
                }

                RequirementGroupData.refDataDic[FISH_GROUP_ID].requirementIdAry = fi;
            }
        }
    }
}