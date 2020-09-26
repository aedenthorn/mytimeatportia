using Harmony12;
using Pathea.Missions;
using System;

namespace MultipleCommerce
{
    public static partial class Main
    {
        [HarmonyPatch(typeof(OrderMissionManager), "GetRandomOrderMission", new Type[] { typeof(Random), typeof(int),typeof(int) })]
        static class OrderMissionManager_GetRandomOrderMission_Patch
        {
            static void Prefix(ref int count, ref int centerLevel, ref IntR ___OrgBigOrderCount, ref IntR ___NpcSpecialOrderCount)
            {
                if (enabled)
                {
                    ___OrgBigOrderCount = new IntR(1, settings.NumberBigOrders);
                    ___NpcSpecialOrderCount = new IntR(0, settings.NumberSpecialOrders);
                    count = settings.NumberCommerceOrders;
                    if (settings.RandomOrderLevel)
                    {
                        centerLevel = new IntR(1, centerLevel).GetValue();
                    }
                }
            }
        }
    }
}