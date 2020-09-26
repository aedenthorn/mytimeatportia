using Harmony12;
using Pathea.Missions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultipleCommerce
{
    public static partial class Main
    {
        [HarmonyPatch(typeof(NpcOrderData), "LoadDataBase", new Type[] { })]
        static class NpcOrderData_LoadDataBase_Patch
        {
            static void Postfix()
            {
                if (!enabled || !settings.AddFishSpecialOrders)
                    return;

                Dbgl("NpcOrderData_LoadDataBase_Patch");

                var nodd = NpcOrderData.refDataDic.Keys.ToArray();
                foreach (int i in nodd)
                {
                    if (i >= 4000000)
                    {
                        NpcOrderData nod = NpcOrderData.refDataDic[i];
                        nod.reqDescSubList.Add(new ReqDescSubData($"{FISH_GROUP_ID}|91320002|1040;1064;1066;1069"));
                        NpcOrderData.refDataDic[i] = nod;
                    }
                }
            }
        }
    }
}