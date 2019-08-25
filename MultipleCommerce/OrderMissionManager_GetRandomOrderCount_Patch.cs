using Harmony12;
using Pathea.Missions;
using System;

namespace MultipleCommerce
{
    public static partial class Main
    {

        // this doesn't work for some reason
        [HarmonyPatch(typeof(OrderMissionManager), "GetRandomOrderCount", new Type[] { typeof(System.Random) })]
        static class OrderMissionManager_GetRandomOrderCount_Patch
        {
            static bool Prefix(ref int __result)
            {
                if (!Main.enabled)
                    return true;
                __result = settings.NumberCommerceOrders;
                return false;
            }
        }
    }
}