using Harmony12;
using Pathea.Missions;
using Pathea.GuildRanking;
using System;
using System.Collections.Generic;

namespace MultipleCommerce
{
    public static partial class Main
    {
        [HarmonyPatch(typeof(MissionManager), "CheckCanReceive", new Type[] { typeof(int), typeof(Action<int>) })]
        static class MissionManager_CheckCanReceive_Patch
        {
            static bool Prefix(List<Mission> ___m_missions_Running, int missionId, ref bool __result)
            {
                if (!enabled)
                    return true;
                if (___m_missions_Running.Find((Mission it) => it.InstanceID == missionId) == null && MissionProcessManager.Self.IsOrderMission(missionId))
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