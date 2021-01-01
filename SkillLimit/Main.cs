using Harmony12;
using Pathea.FeatureNs;
using Pathea.ModuleNs;
using Pathea.PlayerAbilityNs;
using Pathea.UISystemNs.Grid;
using Pathea.UISystemNs.MainMenu;
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityModManagerNet;

namespace SkillLimit
{
    public class Main
    {
        public static Settings settings { get; private set; }
        public static bool enabled;

        private static readonly bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? typeof(Main).Namespace + " " : "") + str);
        }


        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

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
        }

        [HarmonyPatch(typeof(AbilityItem), "AddPoint")]
        internal static class AbilityItem_AddPoint
        {
            private static bool Prefix(AbilityItem __instance, int ___point, ref bool __result)
            {
                if (!enabled)
                    return true;

                __instance.SetPoint(___point + 1);
                __result = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(AbilityItem), "GetNextDesc")]
        internal static class AbilityItem_GetNextDesc
        {
            private static bool Prefix(AbilityItem __instance, ref string __result, AbilityData ___refData)
            {
                if (!enabled)
                    return true;

                string str = TextMgr.GetStr(___refData.descTextId, -1);
                List<object> list = new List<object>();
                for (int i = 0; i < __instance.relatedModifierId.Count; i++)
                {
                    list.AddRange(Module<FeatureModule>.Self.GetNextValueAry(__instance.relatedModifierId[i]));
                }
                __result = string.Format(str, list.ToArray());
                return false;
            }
        }
        
        [HarmonyPatch(typeof(PlayerAbilityUI), "ShowLevelDetail", new Type[] { typeof(int), typeof(int), typeof(int), typeof(bool), typeof(bool) })]
        internal static class PlayerAbilityUI_ShowLevelDetail
        {
            private static void Postfix(int group, int line, int icon, bool show, GameObject ___curLevelDetailParent, GameObject ___nextLevelDetailParent, ref PlayerAbilityDesc ___curLevelDetail, ref PlayerAbilityDesc ___nextLevelDetail, WhiteCat.Tween.Tweener[] ___levelChangeTween)
            {
                if (!enabled)
                    return;

                AbilityItem abilityItem = Module<PlayerAbilityModule>.Self.Get(group, line, icon);
                if (show && abilityItem.IsMax)
                {
                    ___curLevelDetailParent.gameObject.SetActive(true);
                    foreach (WhiteCat.Tween.Tweener tweener2 in ___levelChangeTween)
                    {
                        tweener2.normalizedTime = 0f;
                        tweener2.isForward = true;
                        tweener2.enabled = true;
                    }
                    ___nextLevelDetailParent.gameObject.SetActive(true);
                    ___curLevelDetail.SetDesc(abilityItem.GetIcon(), abilityItem.GetName(), abilityItem.Point, abilityItem.GetDesc());
                    ___nextLevelDetail.SetDesc(abilityItem.GetIcon(), abilityItem.GetName(), abilityItem.Point + 1, abilityItem.GetNextDesc());
                }
            }
        } 

        [HarmonyPatch(typeof(GridPlayerAbility), "Fresh")]
        internal static class GridPlayerAbility_Fresh
        {
            private static void Postfix(GridPlayerAbility __instance, TextMeshProUGUI ___iconLevel, GameObject ___plusButton, Color ___notFullColor, GameObject ___lockIcon, WhiteCat.Tween.Tweener ___tweener, int ___group, int ___line, AbilityItem ___ability)
            {
                if (!enabled)
                    return;

                int point = ___ability.Point;
                ___iconLevel.text = point.ToString();
                ___iconLevel.color = ___notFullColor;
                bool flag2 = Module<PlayerAbilityModule>.Self.IsLocked(___group, ___line);
                bool flag3 = !flag2 && Module<PlayerAbilityModule>.Self.GetPoint() != 0;
			    ___lockIcon.SetActive(flag2);
			    if (!flag3)
			    {
                    __instance.selectableBg.OnDeselect(null);
			    }
                __instance.GetComponentInChildren<GridEventHandler_NoDrag>().DisableClickEvent = !flag3;
			    ___plusButton.SetActive(flag3);
                __instance.clickIcon.material.SetFloat("_Saturation", (float) ((point != 0) ? 1 : 0));
			    ___tweener.normalizedTime = (float) ((point != 0) ? 1 : 0);
                __instance.clickIcon.material.SetColor("_Color", (!flag2) ? Color.white : new Color(0.6f, 0.6f, 0.6f));
            }
        } 
    }
}
