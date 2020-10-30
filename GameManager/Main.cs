using Harmony12;
using Pathea;
using Pathea.ItemSystem;
using Pathea.MiniGameNs;
using Pathea.ModuleNs;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace GameManager
{
    public class Main
    {
        public static Settings settings { get; private set; }
        public static bool enabled;
        
        public static List<int> vacuumed;
        public static int pages = 3;

        private static readonly bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? typeof(Main).Namespace + " " : "") + str);
        }

        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnToggle = OnToggle;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }


        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }
        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {

            GUILayout.Label(string.Format("Slot Speed Multiplier : <b>{0:F2}x</b>", settings.SlotSpeedMult), new GUILayoutOption[0]);
            settings.SlotSpeedMult = GUILayout.HorizontalSlider((float)settings.SlotSpeedMult * 100f, 1f, 100f, new GUILayoutOption[0]) / 100f;

            GUILayout.Space(10);
            GUILayout.Label(string.Format("Slot Cost Multiplier : <b>{0:F1}x</b>", settings.SlotCostMult), new GUILayoutOption[0]);
            settings.SlotCostMult = GUILayout.HorizontalSlider((float)settings.SlotCostMult * 10f, 1f, 1000f, new GUILayoutOption[0]) / 10f;

            GUILayout.Space(10);
            GUILayout.Label(string.Format("Slot Reward Multiplier : <b>{0:F1}x</b>", settings.SlotRewardMult), new GUILayoutOption[0]);
            settings.SlotRewardMult = GUILayout.HorizontalSlider((float)settings.SlotRewardMult * 10f, 1f, 1000f, new GUILayoutOption[0]) / 10f;

            GUILayout.Space(20);
            GUILayout.Label(string.Format("Balloon Bullet Multiplier : <b>{0:F1}x</b>", settings.BalloonBulletCountMult), new GUILayoutOption[0]);
            settings.BalloonBulletCountMult = GUILayout.HorizontalSlider((float)settings.BalloonBulletCountMult * 10f, 1f, 1000f, new GUILayoutOption[0]) / 10f;

            GUILayout.Space(10);
            GUILayout.Label(string.Format("Balloon Score Multiplier : <b>{0:F1}x</b>", settings.BalloonScoreMult), new GUILayoutOption[0]);
            settings.BalloonScoreMult = GUILayout.HorizontalSlider((float)settings.BalloonScoreMult * 10f, 1f, 1000f, new GUILayoutOption[0]) / 10f;

            GUILayout.Space(10);
            GUILayout.Label(string.Format("Balloon Scale Multiplier : <b>{0:F1}x</b>", settings.BalloonScaleMult), new GUILayoutOption[0]);
            settings.BalloonScaleMult = GUILayout.HorizontalSlider((float)settings.BalloonScaleMult * 10f, 1f, 100f, new GUILayoutOption[0]) / 10f;

            GUILayout.Space(10);
            GUILayout.Label(string.Format("Balloon Reward Multiplier : <b>{0:F1}x</b>", settings.BalloonRewardMult), new GUILayoutOption[0]);
            settings.BalloonRewardMult = GUILayout.HorizontalSlider((float)settings.BalloonRewardMult * 10f, 1f, 1000f, new GUILayoutOption[0]) / 10f;

            GUILayout.Space(20);
            GUILayout.Label(string.Format("Darts Rotation Speed Multiplier : <b>{0:F1}x</b>", settings.DartSpeedMult), new GUILayoutOption[0]);
            settings.DartSpeedMult = GUILayout.HorizontalSlider((float)settings.DartSpeedMult * 10f, 1f, 100f, new GUILayoutOption[0]) / 10f;

            GUILayout.Space(10);
            GUILayout.Label(string.Format("Darts Reward Multiplier : <b>{0:F1}x</b>", settings.DartRewardMult), new GUILayoutOption[0]);
            settings.DartRewardMult = GUILayout.HorizontalSlider((float)settings.DartRewardMult * 10f, 1f, 1000f, new GUILayoutOption[0]) / 10f;

        }


        [HarmonyPatch(typeof(DartCtr), "Enter")]
        static class Dart_Enter_Patch
        {
            static void Prefix(ref float ___DartBoardRotationSpeed)
            {
                Dbgl("Enter Dart Ctr");

                ___DartBoardRotationSpeed = 12 * settings.DartSpeedMult;

            }
        }
        [HarmonyPatch(typeof(MiniGameRewardMgr), "GetDartReward")]
        static class MiniGameRewardMgr_GetDartReward_Patch
        {
            static void Postfix(ref List<ItemObject> __result)
            {
                for (int i = 0; i < __result.Count; i++)
                {
                    typeof(ItemObject).GetField("number", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__result[i], (int)Math.Round((int)typeof(ItemObject).GetField("number", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__result[i]) * settings.DartRewardMult));
                }
            }
        }

        [HarmonyPatch(typeof(BalloonCtr), "Enter")]
        static class BalloonCtr_Enter_Patch
        {
            static void Prefix(ref int ___BulletCount)
            {
                Dbgl("Enter Balloon Ctr");

                ___BulletCount = (int)Math.Round(20 * settings.BalloonBulletCountMult);

            }
        }

        [HarmonyPatch(typeof(BallonGenerator), "GetScore")]
        static class BallonGenerator_GetScore_Patch
        {
            static void Postfix(ref float __result)
            {
                Dbgl("Balloon Generator GetScore");

                __result = (float)Math.Round(__result * settings.BalloonScoreMult);

            }
        }

        [HarmonyPatch(typeof(BallonGenerator), "Initiazation")]
        static class BallonGenerator_Initiazation_Patch
        {
            static void Prefix(ref BalloonObject balloon)
            {
                Dbgl("Balloon Generator Initiazation");

                balloon.scaleMin = (float)Math.Round(balloon.scaleMin * settings.BalloonScaleMult);
                balloon.scaleMax = (float)Math.Round(balloon.scaleMax * settings.BalloonScaleMult);

            }
        }

        [HarmonyPatch(typeof(MiniGameRewardMgr), "GetBalloonReward")]
        static class MiniGameRewardMgr_GetBalloonReward_Patch
        {
            static void Postfix(ref List<ItemObject> __result)
            {
                for (int i = 0; i < __result.Count; i++)
                {
                    typeof(ItemObject).GetField("number", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__result[i], (int)Math.Round((int)typeof(ItemObject).GetField("number", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__result[i]) * settings.BalloonRewardMult));
                }
            }
        }

        [HarmonyPatch(typeof(SlotMachineCtr), "Enter")]
        static class SlotMachineCtr_Enter_Patch
        {
            static void Prefix(ref List<int> ___moneyRate)
            {
                Dbgl("Enter Slot Ctr");

                ___moneyRate = new List<int>
                {
                    (int)Math.Round(-5 * settings.SlotCostMult),
                    (int)Math.Round(-10 * settings.SlotCostMult),
                    (int)Math.Round(-50 * settings.SlotCostMult)
                };
            }
        }

        [HarmonyPatch(typeof(SlotMachineScroller), "StartWork")]
        static class SlotMachineScroller_StartWork_Patch
        {
            static void Prefix(ref List<object> ___scrolls)
            {
                Dbgl("Got scroller");

                for (int i = 0; i < ___scrolls.Count; i++)
                {
                    Dbgl($"getting scrolls {i}");
                    if (___scrolls[i] == null)
                        continue;
                    ___scrolls[i].GetType().GetField("speed", BindingFlags.Public | BindingFlags.Instance).SetValue(___scrolls[i], 360 * settings.SlotSpeedMult);
                }
            }
        }

        [HarmonyPatch(typeof(MiniGameRewardMgr), "GetSlotMachineReward", new Type[] { typeof(int), typeof(int) })]
        static class MiniGameRewardMgr_GetSlotMachineReward_Patch
        {
            static void Postfix(ref List<ItemObject> __result)
            {
                for(int i = 0; i < __result.Count; i++)
                {
                    typeof(ItemObject).GetField("number", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__result[i], (int)Math.Round((int)typeof(ItemObject).GetField("number", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__result[i]) * settings.SlotRewardMult));
                }
            }
        }

        [HarmonyPatch(typeof(MiniGameRewardMgr), "GetSlotMachineReward", new Type[] { typeof(string), typeof(int) })]
        static class MiniGameRewardMgr_GetSlotMachineReward_Patch2
        {
            static void Postfix(ref List<ItemObject> __result)
            {
                for(int i = 0; i < __result.Count; i++)
                {
                    typeof(ItemObject).GetField("number", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__result[i], (int)Mathf.Round((int)typeof(ItemObject).GetField("number", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__result[i]) * settings.SlotRewardMult));
                }
            }
        }
    }
}
