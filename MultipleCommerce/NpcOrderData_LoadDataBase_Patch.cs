using Harmony12;
using Pathea.Missions;
using System;
using System.Collections.Generic;

namespace MultipleCommerce
{
    public static partial class Main
    {
        [HarmonyPatch(typeof(NpcOrderData), "LoadDataBase", new Type[] { })]
        static class NpcOrderData_LoadDataBase_Patch
        {
            static void Postfix()
            {
                if (!enabled)
                    return;
                Dictionary<int, NpcOrderData> nodd = NpcOrderData.refDataDic;
                foreach (KeyValuePair<int,NpcOrderData> kvp in nodd)
                {
                    if (kvp.Key >= 4000000 && settings.AddFishSpecialOrders)
                    {
                        NpcOrderData nod = kvp.Value;
                        nod.reqDescSubList.Add(new ReqDescSubData($"{FISH_GROUP_ID}|91320002|1040;1064;1066;1069"));
                        NpcOrderData.refDataDic[kvp.Key] = nod;
                    }
                }
            }
        }
    }
}