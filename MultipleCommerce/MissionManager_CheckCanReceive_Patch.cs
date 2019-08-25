using Harmony12;
using Pathea.Missions;
using Pathea.GuildRanking;
using System;

namespace MultipleCommerce
{
    public static partial class Main
    {
        [HarmonyPatch(typeof(MissionManager), "CheckCanReceive", new Type[] { typeof(int), typeof(Action<int>) })]
        static class MissionManager_CheckCanReceive_Patch
        {
            static bool Prefix(int missionId, ref bool __result)
            {
                if (!Main.enabled)
                    return true;
                if (MissionProcessManager.Self.IsOrderMission(missionId))
                {
                    int playerReceiveOrderCount = GuildRankingManager.Self.GetPlayerReceiveOrderCount();
                    if (playerReceiveOrderCount != 0)
                    {
                        __result = true;
                        return false;
                    }
                }
                return true;
            }
        }
    }
}