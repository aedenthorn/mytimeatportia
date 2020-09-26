using Harmony12;
using Pathea;
using Pathea.ItemSystem;
using Pathea.Missions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultipleCommerce
{
    public static partial class Main
    {
        private static int[] FishInts = { 4000244, 4000248, 4000249, 4000247, 4000250, 4000253, 4000245, 4000255, 4000252, 4000254, 4000256 };

        [HarmonyPatch(typeof(RequirementData), "LoadDataBase", new Type[] { })]
        static class RequirementData_LoadDataBase_Patch
        {
            static void Postfix()
            {
                if (!enabled || !settings.AddFishSpecialOrders)
                    return;
                Dbgl("NpcOrderData_LoadDataBase_Patch");

                int index = DIVERSE_ID_COUNTER;
                foreach (int f in FishInts)
                {
                    RequirementData.refDataDic.Add(index, new RequirementData(index++, f, new DoubleInt(1, 1), new DoubleInt(2000, 2000), new DoubleInt(150, 150), new DoubleInt(70, 70), new DoubleInt(85, 85), 5, -1, 100, 10, new List<int> { }, new List<int> { }));
                }
            }
        }
    }
}