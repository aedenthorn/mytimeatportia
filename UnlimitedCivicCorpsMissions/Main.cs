using Harmony12;
using Pathea;
using Pathea.ConfigNs;
using Pathea.DungeonModuleNs;
using Pathea.DungeonStateNs;
using Pathea.GuildRanking;
using Pathea.ModuleNs;
using Pathea.PlayerMissionNs;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityModManagerNet;
using static Harmony12.AccessTools;

namespace UnlimitedCivicCorpsMissions
{
    public class Main
    {
        private static bool isDebug = false;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "UnlimitedCivicCorpsMissions " : "") + str);
        }
        public static bool enabled;
        public static Settings settings { get; private set; }

        // Send a response to the mod manager about the launch status, success or not.
        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            return true;
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label(string.Format("Max numer of missions : <b>{0:F1}</b>", settings.MaxMissions), new GUILayoutOption[0]);
            settings.MaxMissions = (int)GUILayout.HorizontalSlider((float)settings.MaxMissions, 1f, 10f, new GUILayoutOption[0]);
        }

        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }

        [HarmonyPatch(typeof(PlayerMissionUICtr), "FreshBtnState")]
        static class PlayerMissionUICtr_FreshBtnState_Patch
        {
            static void Postfix(PlayerMissionUICtr __instance, MissionData ___curMissionData,
                ref GameObject ___submitMissionBtn, ref TextMeshProUGUI ___canotSubmitText)
            {
                if (!enabled || ___curMissionData == null)
                {
                    return;
                }

                int playerWorkshopRepLevel = Module<GuildRankingManager>.Self.GetPlayerWorkshopRepLevel();
                if (playerWorkshopRepLevel >= ___curMissionData.Level &&
                    !(___curMissionData.MissionType == 2 && !CheckDegunPassed(___curMissionData.DungeonName,
                          ___curMissionData.DungeonLevel)))
                {
                    ___submitMissionBtn.SetActive(true);
                    ___canotSubmitText.gameObject.SetActive(false);
                }
            }

            private static bool CheckDegunPassed(string scenename, int level)
            {
                if (!Module<DungeonStateMgr>.Self.CheckDungeonUnlocked(scenename))
                {
                    return false;
                }
                int maxClearlevel = Module<TrialRandomDungeonManager>.Self.GetMaxClearlevel(scenename);
                return maxClearlevel >= level;
            }
        }

        [HarmonyPatch(typeof(PlayerMissionMgr), "PublishMission")]
        static class PlayerMissionMgr_Patch
        {
            static void Prefix()
            {
                if (!enabled)
                    return;

                FieldRef<OtherConfig,int> playerMissionMaxCount = FieldRefAccess<OtherConfig, int>("playerMissionMaxCount");
                playerMissionMaxCount(OtherConfig.Self) = settings.MaxMissions;
            }
        }

        private static Dictionary<int,object[]> newMissions = new Dictionary<int, object[]>(){
            { 1015,new object[] { 1015, 1, 1, -1, 283024, "", 4, new int[] { 50, 200 }, new int[] { 2, 6 }, new string[] { "4000022" }, "", null, -1, "", 0 } },
            { 1016,new object[] { 1016, 1, 2, -1, 283024, "", 14, new int[] { 15, 50 }, new int[] { 2, 6 }, new string[] { "4000165" }, "", null, 1100411, "", 0 } },
            { 1017,new object[] { 1017, 1, 3, -1, 283024, "", 12, new int[] { 15, 50 }, new int[] { 2, 6 }, new string[] { "4000140" }, "", null, 1100452, "", 0 } },
        };

        [HarmonyPatch(typeof(MissionData), "List")]
        [HarmonyPatch(MethodType.Getter)]
        static class DbReader_Read_Patch
        {
            static void Postfix(ref List<MissionData> __result)
            {
                if (!enabled)
                    return;

                Dbgl("dbreader patch");

                List<int> ids = new List<int>();

                foreach(MissionData md in __result)
                {
                    ids.Add(md.Id);
                }

                Dbgl($"newMissions: {newMissions.Count} ids: {ids.Count}");

                foreach (KeyValuePair<int, object[]> kvp in newMissions)
                {
                    Dbgl(kvp.Key+"");
                    if (!ids.Contains(kvp.Key))
                    {
                        Dbgl(kvp.Key + " adding");
                        __result.Add(MakeMission(kvp.Value));
                    }
                }

            }
        }

        private static string[] mdfields = { "Id", "MissionType", "Level", "Title", "Desc", "TargetLocation", "CostMoney", "NumRange", "TimeRange", "rewardStr", "DungeonName", "DungeonLevel", "MissionRequired", "IconPath", "FaildMissionRate"};

        static MissionData MakeMission(object[] args)
        {
            MissionData md = new MissionData();
            for(int i = 0; i < args.Length; i++)
            {
                typeof(MissionData).GetProperty(mdfields[i]).SetValue(md, args[i], null);

            }

            return md;
        }

        /*
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int startIndex = -1;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_3)
                {
                    startIndex = i;
                    break;
                }
            }

            if (startIndex > -1)
            {
                codes[startIndex] = new CodeInstruction(OpCodes.Ldc_I4, codes[startIndex-1].operand);
            }
            return codes.AsEnumerable();
        }
        */
    }
}