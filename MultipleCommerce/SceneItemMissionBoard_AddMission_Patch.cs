using Harmony12;
using Pathea.Missions;
using PatheaScriptExt;
using UnityEngine;

namespace MultipleCommerce
{
    public static partial class Main
    {
        [HarmonyPatch(typeof(SceneItemMissionBoard), "AddMission")]
        static class SceneItemMissionBoard_AddMission_Patch
        {
            static bool Prefix(ref SceneItemMissionBoard __instance, Pathea.Missions.Mission m, System.Random rand = null)
            {
                if (!enabled)
                    return true;
                if (rand == null)
                {
                    rand = new System.Random();
                }

                double xd = 13.932;
                __instance.AddOrRemoveIconStack(0, 0, -1);
                foreach (MissionContentBrief missionContentBrief in m.choosableContent)
                {
                    if (__instance.HasEmptySeat)
                    {
                        string modelPath = __instance.GenBoardItemPath(missionContentBrief.missionId);
                        __instance.CreateBoardItem(modelPath, missionContentBrief.missionId, missionContentBrief.chooseIndex, __instance.FindEmptySeat(rand), rand);
                    }
                    else
                    {
                        UnityEngine.Debug.LogError(string.Concat(new object[]
                        {
                            "SceneItemMissionBoard not enough seats for mission! Adding new seat!",
                            missionContentBrief.missionId,
                            " ",
                            missionContentBrief.chooseIndex
                        }));
                        string modelPath = __instance.GenBoardItemPath(missionContentBrief.missionId);

                        xd -= 0.002;
                        BoardItemSeat bis = __instance.AllBoardItemSeat[rand.Next(__instance.AllBoardItemSeat.Count)];
                        BoardItemSeat bis2 = new BoardItemSeat(new Vector3((float)xd, 0.917f+(float)rand.NextDouble()*0.9f, 9.15f + (float)rand.NextDouble() * 2.4f), bis.eulerRot);
                        __instance.AllBoardItemSeat.Add(bis2);
                        __instance.MyData.allBoardItemSeat.Add(bis2);
                        __instance.CreateBoardItem(modelPath, missionContentBrief.missionId, missionContentBrief.chooseIndex, bis2, rand);
                    }
                }

                return false;
            }
        }
    }
}