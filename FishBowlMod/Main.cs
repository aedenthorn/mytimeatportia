using Harmony12;
using Pathea;
using Pathea.HomeNs;
using Pathea.HomeViewerNs;
using Pathea.InputSolutionNs;
using Pathea.ItemSystem;
using Pathea.MessageSystem;
using Pathea.ModuleNs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityModManagerNet;
using static Harmony12.AccessTools;

namespace FishBowlMod
{
    public class Main
    {
        private static readonly bool isDebug = true;
        private static bool enabled = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "DatabaseExtension " : "") + str);
        }
        private static void Load(UnityModManager.ModEntry modEntry)
        {
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
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
        }


        [HarmonyPatch(typeof(FishBowlUnitViewer), "FeedFish")]
        static class FishBowlUnitViewer_PlayerTarget_TriggerEvent_Patch
        {
            static void Postfix(FishBowlUnitViewer __instance, FishBowlUnit ___fishBowlUnit)
            {
                Dbgl("Fed fish");
                ShowHunger(__instance, ___fishBowlUnit);
            }
        }

        [HarmonyPatch(typeof(FishBowlUnitViewer), "FreshInteractHint")]
        static class FishBowlUnitViewer_FreshInteractHint_Patch
        {
            static bool Prefix(FishBowlUnitViewer __instance, FishBowlUnit ___fishBowlUnit)
            {
                ShowHunger(__instance, ___fishBowlUnit);
                return false;
            }
        }

        static void ShowHunger(FishBowlUnitViewer __instance, FishBowlUnit ___fishBowlUnit)
        {
            FieldRef<UnitViewer, PlayerTargetMultiAction> CurPlayerTargetRef = FieldRefAccess<UnitViewer, PlayerTargetMultiAction>("playerTarget");

            PlayerTargetMultiAction CurPlayerTarget = CurPlayerTargetRef(__instance);

            if (CurPlayerTarget == null)
                return;

            Dbgl("Showing Hunger");

            FishBowl fishBowl = AccessTools.FieldRefAccess<FishBowlUnit, FishBowl>(___fishBowlUnit, "fishBowl");
            float max = AccessTools.FieldRefAccess<FishBowl, FishBowl.FishBowlData>(fishBowl, "data").maxVolumn;
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
                string nextRepro = next < int.MaxValue ? $" (Next in {next} days)" : "";
                fishs.Add($"{kvp.Value.Count} {Module<ItemDataMgr>.Self.GetItemName(kvp.Key)}{nextRepro}");
            }


            if (___fishBowlUnit.HasDeadFish())
            {
                CurPlayerTarget.SetAction(ActionType.ActionInteract, TextMgr.GetStr(300318, -1), ActionTriggerMode.Normal);
            }
            else if (___fishBowlUnit.FishCount > 0)
            {
                CurPlayerTarget.SetAction(ActionType.ActionInteract, $"{TextMgr.GetStr(300316, -1)} ({count}/{max})", ActionTriggerMode.Normal);
                CurPlayerTarget.SetAction(ActionType.ActionMoveBack, $"{string.Join("\r\n",fishs.ToArray())}", ActionTriggerMode.Normal);
                CurPlayerTarget.SetAction(ActionType.ActionFavor, $"Hungry: {hungry}, Avg. {Math.Round(chp / mhp * 100)}%, Low {Math.Round(lhp * 100)}%", ActionTriggerMode.Normal);
            }
            else
            {
                CurPlayerTarget.RemoveAction(ActionType.ActionInteract, ActionTriggerMode.Normal);
            }
            return;
        }

    }
}
