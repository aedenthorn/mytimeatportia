using Harmony12;
using Hont;
using Pathea.HomeNs;
using Pathea.UISystemNs;
using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityModManagerNet;

namespace PlantMod
{
    public static partial class Main
    {
        private static Settings settings;
        private static bool enabled;
        private static bool isDebug = false;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                UnityEngine.Debug.Log((pref ? "PlantMod " : "") + str);
        }
        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

        }


        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label(string.Format("Plant Growth Speed Multiplier: <b>{0:F1}x</b>", settings.plantGrowMult), new GUILayoutOption[0]);
            settings.plantGrowMult = GUILayout.HorizontalSlider(settings.plantGrowMult * 10f, 0f, 1000f, new GUILayoutOption[0]) / 10f;
            GUILayout.Space(10f);
            GUILayout.Label(string.Format("Nutrient Consumption Multiplier: <b>{0:F2}</b>", settings.nutrientConsumeMult), new GUILayoutOption[0]);
            settings.nutrientConsumeMult = GUILayout.HorizontalSlider(settings.nutrientConsumeMult * 100f, 0f, 1000f, new GUILayoutOption[0]) / 100f;
            GUILayout.Space(10f);
        }


        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }

        [HarmonyPatch(typeof(NutrientContainerUnit), "ChangeNutrient")]
        static class NutrientContainerUnit_ChangeNutrient_Patch
        {
            static void Prefix(NutrientContainerUnit __instance, ref float value)
            {
                if (!enabled || __instance.GetType() != typeof(PlantingBoxUnit) || value > 0)
                    return;

                value *= settings.nutrientConsumeMult;

            }
        }
        [HarmonyPatch(typeof(Plant), "CalculateGrowSeconds")]
        static class PlantingBoxUnit_Grow_Patch
        {
            static void Prefix(ref float second)
            {
                if (!enabled)
                    return;

                second *= settings.plantGrowMult;
            }
        }

        [HarmonyPatch(typeof(PlantingBoxInfoUI), "FreshGrowDisplay")]
        static class PlantingBoxInfoUI_FreshGrowDisplay_Patch
        {
            static void Postfix(PlantingBoxInfoUI __instance, bool isInit, ref TextMeshProUGUI ___progText, bool ___isEnable, bool ___riped, bool ___isBadSeason, GameTimeSpan ___timeToRipe)
            {
                if (!enabled)
                    return;
                if (!isInit)
                {
                    if (!__instance.enabled)
                    {
                        return;
                    }
                    if (!___isEnable)
                    {
                        return;
                    }
                }

                GameTimeSpan gts = new GameTimeSpan((long)Math.Round(___timeToRipe.Ticks/settings.plantGrowMult));

                if (!___riped && !___isBadSeason && gts.TotalDays <= 99.0)
                {
                    if (gts.TotalDays < 1)
                    {
                        ___progText.text = string.Format(TextMgr.GetStr(100373, -1), gts.Hours, gts.Minutes);
                    }
                    else if (gts.TotalDays < 2)
                    {
                        ___progText.text = string.Format($"{TextMgr.GetStr(100972, -1)} {TextMgr.GetStr(100373, -1)}", (int)gts.TotalDays, gts.Hours, gts.Minutes).Replace("(s)","").Replace("(e)","");
                    }
                    else
                    {
                        ___progText.text = string.Format($"{TextMgr.GetStr(100972, -1)} {TextMgr.GetStr(100373, -1)}", (int)gts.TotalDays, gts.Hours, gts.Minutes).Replace("(s)","s").Replace("(e)", "e");
                    }
                }
            }
        }
    }
}
