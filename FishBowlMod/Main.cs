using Harmony12;
using Pathea;
using Pathea.HomeNs;
using Pathea.HomeViewerNs;
using Pathea.InputSolutionNs;
using Pathea.ItemSystem;
using Pathea.ModuleNs;
using Pathea.OptionNs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace FishBowlMod
{
    public class Main
    {
        private static readonly bool isDebug = true;
        private static bool enabled = true;
        private static string[] strings;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? typeof(Main).Namespace + " " : "") + str);
        }
        private static void Load(UnityModManager.ModEntry modEntry)
        {
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnToggle = OnToggle;

            reloadStrings();

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
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
        }


        private static void reloadStrings()
        {
            string lang = "";

            switch ((int)Singleton<OptionsMgr>.Instance.LanguageGame)
            {
                case 0:
                    lang = ("Chinese");
                    break;
                case 1:
                    lang = ("English");
                    break;
                case 2:
                    lang = ("German");
                    break;
                case 3:
                    lang = ("French");
                    break;
                case 4:
                    lang = ("T_Chinese");
                    break;
                case 5:
                    lang = ("Italian");
                    break;
                case 6:
                    lang = ("Spanish");
                    break;
                case 7:
                    lang = ("Japanese");
                    break;
                case 8:
                    lang = ("Russian");
                    break;
                case 9:
                    lang = ("Turkish");
                    break;
                case 10:
                    lang = ("Korean");
                    break;
                case 11:
                    lang = ("Portuguese");
                    break;
                case 12:
                    lang = ("Vietnamese");
                    break;
            }
            if (lang == "")
                lang = ("English");

            string path = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\assets\\{lang}.txt";
            if (!File.Exists(path))
            {
                Dbgl("No translation file found at " + path);
                return;
            }
            Dbgl($"Using {lang} language file");
            strings = File.ReadAllLines(path);
        }

        [HarmonyPatch(typeof(FishBowlUnitViewer), "FeedFish")]
        static class FishBowlUnitViewer_PlayerTarget_TriggerEvent_Patch
        {
            static void Postfix(FishBowlUnitViewer __instance, FishBowlUnit ___fishBowlUnit)
            {
                if (!enabled)
                    return;
                Dbgl("Fed fish");
                ShowHunger(__instance, ___fishBowlUnit);
            }
        }

        [HarmonyPatch(typeof(FishBowlUnitViewer), "FreshInteractHint")]
        static class FishBowlUnitViewer_FreshInteractHint_Patch
        {
            static bool Prefix(FishBowlUnitViewer __instance, FishBowlUnit ___fishBowlUnit)
            {
                if (!enabled)
                    return true;
                ShowHunger(__instance, ___fishBowlUnit);
                return false;
            }
        }

        static void ShowHunger(FishBowlUnitViewer __instance, FishBowlUnit ___fishBowlUnit)
        {

            PlayerTargetMultiAction CurPlayerTarget = (PlayerTargetMultiAction)typeof(UnitViewer).GetProperty("CurPlayerTarget", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance, null);

            Dbgl("Showing Hunger");

            FishBowl fishBowl = AccessTools.FieldRefAccess<FishBowlUnit, FishBowl>(___fishBowlUnit, "fishBowl");
            float max = AccessTools.FieldRefAccess<FishBowl, FishBowl.FishBowlData>(fishBowl, "data").volumn;
            float maxmax = AccessTools.FieldRefAccess<FishBowl, FishBowl.FishBowlData>(fishBowl, "data").maxVolumn;
            int count = ___fishBowlUnit.FishCount;
            int hungry = 0;
            float chp = 0;
            float mhp = 0;
            float lhp = 0;
            Dictionary<int, List<FishInFishBowl>> fcounts = new Dictionary<int, List<FishInFishBowl>>();
            for (int i = 0; i < count; i++)
            {
                FishInFishBowl fish = ___fishBowlUnit.GetFish(i);

                if (fcounts.ContainsKey(fish.FishId))
                {
                    fcounts[fish.FishId].Add(fish);
                }
                else
                {
                    fcounts.Add(fish.FishId, new List<FishInFishBowl>() { fish });
                }

                chp += fish.CurHp;
                mhp += fish.MaxHp;
                if (fish.CurHp > 0 && (lhp <= 0 || fish.CurHp / fish.MaxHp < lhp))
                    lhp = fish.CurHp / fish.MaxHp;

                if (fish.IsHunger)
                {
                    hungry++;
                }
            }

            List<string> fishs = new List<string>();
            foreach(KeyValuePair<int,List<FishInFishBowl>> kvp in fcounts)
            {
                int next = int.MaxValue;
                List<FishInFishBowl> list = kvp.Value;
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (!list[i].IsDead && (!list[i].IsHunger || list[i].CanReproduce))
                    {
                        for (int j = 0; j < i; j++)
                        {
                            if (!list[j].IsDead && list[j].FishId == list[i].FishId && (!list[i].IsHunger || list[i].CanReproduce))
                            {
                                FishData data1 = AccessTools.FieldRefAccess<FishInFishBowl, FishData>(list[i], "data");
                                int reproduce1 = AccessTools.FieldRefAccess<FishInFishBowl, int>(list[i], "reproduceDayCount");
                                int thisNext1 = data1.DayToReproduce - reproduce1;
                                FishData data2 = AccessTools.FieldRefAccess<FishInFishBowl, FishData>(list[j], "data");
                                int reproduce2 = AccessTools.FieldRefAccess<FishInFishBowl, int>(list[j], "reproduceDayCount");
                                int thisNext2 = data2.DayToReproduce - reproduce2;
                                int thisNext = Math.Max(thisNext1, thisNext2);
                                if (thisNext < next)
                                    next = thisNext;
                            }
                        }
                    }
                }
                string nextRepro = next < int.MaxValue ? string.Format(strings[0], next) : "";
                fishs.Add($"{kvp.Value.Count} {Module<ItemDataMgr>.Self.GetItemName(kvp.Key)}{nextRepro}");
            }


            if (___fishBowlUnit.HasDeadFish())
            {
                CurPlayerTarget.SetAction(ActionType.ActionInteract, TextMgr.GetStr(300318, -1), ActionTriggerMode.Normal);
            }
            else if (___fishBowlUnit.FishCount > 0)
            {
                string full = "";
                if (count >= maxmax)
                {
                    full = strings[1];
                }
                else if (count >= max)
                {
                    full = strings[2];
                }
                CurPlayerTarget.SetAction(ActionType.ActionInteract, $"{TextMgr.GetStr(300316, -1)} ({count}/{maxmax}{full})", ActionTriggerMode.Normal);
                CurPlayerTarget.SetAction(ActionType.ActionMoveBack, $"{string.Join("\r\n",fishs.ToArray())}", ActionTriggerMode.Normal);
                CurPlayerTarget.SetAction(ActionType.ActionFavor, string.Format(strings[3], hungry, Math.Round(chp / mhp * 100), Math.Round(lhp * 100)), ActionTriggerMode.Normal);
            }
            else
            {
                CurPlayerTarget.RemoveAction(ActionType.ActionInteract, ActionTriggerMode.Normal);
                CurPlayerTarget.RemoveAction(ActionType.ActionMoveBack, ActionTriggerMode.Normal);
                CurPlayerTarget.RemoveAction(ActionType.ActionFavor, ActionTriggerMode.Normal);
            }
            return;
        }

    }
}
