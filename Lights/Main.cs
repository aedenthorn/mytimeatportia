using Harmony12;
using Pathea;
using Pathea.HomeNs;
using Pathea.HomeViewerNs;
using Pathea.LightMgr;
using Pathea.ModuleNs;
using Pathea.ScenarioNs;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;
using static Harmony.AccessTools;

namespace Lights
{
    public class Main
    {
        public static Settings settings { get; private set; }
        public static bool enabled;
        private static float labelWidth = 250f;
        private static string[] shadowResolutions = new string[]
        {
            "Default",
            "Low",
            "Medium",
            "High",
            "Very High"
        };

        private static readonly bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "Lights " : "") + str);
        }

        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnToggle = OnToggle;
            modEntry.OnHideGUI = HideGUI;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            SceneManager.activeSceneChanged += ChangeScene;
        }

        private static void HideGUI(UnityModManager.ModEntry obj)
        {
            if (Module<ScenarioModule>.Self != null && Module<ScenarioModule>.Self.CurrentScenarioName == "Main")
            {
                RefreshWorkshopLights();
            }

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
            if (Module<ScenarioModule>.Self != null && Module<ScenarioModule>.Self.CurrentScenarioName == "Main")
            {
                RefreshWorkshopLights();
            }
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label(string.Format("Lamp Light Range: <b>{0:F1}</b>", settings.lampLightRange), new GUILayoutOption[0]);
            settings.lampLightRange = GUILayout.HorizontalSlider(settings.lampLightRange * 10f, 0f, 10000f, new GUILayoutOption[0]) / 10f;
            GUILayout.Space(10f);
            GUILayout.Label(string.Format("Lamp Light Intensity: <b>{0:F2}</b>", settings.lampLightIntensity), new GUILayoutOption[0]);
            settings.lampLightIntensity = GUILayout.HorizontalSlider(settings.lampLightIntensity * 100f, 0f, 1000f, new GUILayoutOption[0]) / 100f;
            GUILayout.Space(10f);
            GUILayout.Label(string.Format("Lamp Bounce Intensity: <b>{0:F2}</b>", settings.lampBounceIntensity), new GUILayoutOption[0]);
            settings.lampBounceIntensity = GUILayout.HorizontalSlider(settings.lampBounceIntensity * 100f, 0f, 1000f, new GUILayoutOption[0]) / 100f;
            GUILayout.Space(10f);
            GUILayout.Label(string.Format("Lamp Light Color"), new GUILayoutOption[0]);
            GUILayout.BeginHorizontal();
            GUILayout.Space(20f);
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                GUILayout.Label(string.Format("Red: <b>{0:F2}</b> ", settings.lampColorR), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
                settings.lampColorR = GUILayout.HorizontalSlider((float)settings.lampColorR * 100f, 0, 100f) / 100f;
                GUILayout.EndHorizontal();
                GUILayout.Space(10f);
                GUILayout.BeginHorizontal();
                GUILayout.Label(string.Format("Green: <b>{0:F2}</b> ", settings.lampColorG), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
                settings.lampColorG = GUILayout.HorizontalSlider((float)settings.lampColorG * 100f, 0, 100f) / 100f;
                GUILayout.EndHorizontal();
                GUILayout.Space(10f);
                GUILayout.BeginHorizontal();
                GUILayout.Label(string.Format("Blue: <b>{0:F2}</b> ", settings.lampColorB), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
                settings.lampColorB = GUILayout.HorizontalSlider((float)settings.lampColorB * 100f, 0, 100f) / 100f;
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(20f);

            settings.lampsCastShadows = GUILayout.Toggle(settings.lampsCastShadows, "Lamps Cast Shadows", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            if (settings.lampsCastShadows)
            {
                GUILayout.Label(string.Format("Lamp Shadow Strength: <b>{0:F2}</b>", settings.lampShadowStrength), new GUILayoutOption[0]);
                settings.lampShadowStrength = GUILayout.HorizontalSlider(settings.lampShadowStrength * 100f, 0f, 100f, new GUILayoutOption[0]) / 100f;
                GUILayout.Space(10f);
                GUILayout.Label(string.Format("Lamp Shadow Near Plane: <b>{0:F1}</b>", settings.lampShadowNearPlane), new GUILayoutOption[0]);
                settings.lampShadowNearPlane = GUILayout.HorizontalSlider(settings.lampShadowNearPlane * 10f, 1f, 100f, new GUILayoutOption[0]) / 10f;
                GUILayout.Space(10f);
                GUILayout.Label(string.Format("Lamp Shadow Bias: <b>{0:F2}</b>", settings.lampShadowBias), new GUILayoutOption[0]);
                settings.lampShadowBias = GUILayout.HorizontalSlider(settings.lampShadowBias * 100f, 0f, 200f, new GUILayoutOption[0]) / 100f;
                GUILayout.Space(10f);
                GUILayout.Label(string.Format("Lamp Shadow Normal Bias: <b>{0:F2}</b>", settings.lampShadowNormalBias), new GUILayoutOption[0]);
                settings.lampShadowNormalBias = GUILayout.HorizontalSlider(settings.lampShadowNormalBias * 100f, 0f, 300f, new GUILayoutOption[0]) / 100f;
                GUILayout.Space(10f);
                GUILayout.Label(string.Format("Lamp Shadow Resolution: <b>{0}</b>", shadowResolutions[settings.lampShadowResolution+1]), new GUILayoutOption[0]);
                settings.lampShadowResolution = (int)GUILayout.HorizontalSlider(settings.lampShadowResolution, -1f, 3f, new GUILayoutOption[0]);
                GUILayout.Space(10f);
            }
        }

        private static void ChangeScene(Scene arg0, Scene arg1)
        {
            if (!enabled || arg1.name != "Main")
                return;
            RefreshWorkshopLights();
        }

        private static void RefreshWorkshopLights()
        {
            LayeredRegion layeredRegion = (LayeredRegion)typeof(FarmModule).GetField("layeredRegion", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Module<FarmModule>.Self);
            Region itemLayer = (Region)typeof(LayeredRegion).GetField("itemLayer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(layeredRegion);
            List<object> slots = AccessTools.FieldRefAccess<Region, List<object>>(itemLayer, "slots");

            foreach (object slot in slots)
            {
                UnitObjInfo unitObjInfo = (UnitObjInfo)slot.GetType().GetField("unitObjInfo").GetValue(slot);
                GameObject go = unitObjInfo.go;
                RefreshOneLight(go);

            }
        }

        private static void RefreshOneLight(GameObject go)
        {
            if (go == null)
                return;

            Light light = go.GetComponentInChildren<Light>();
            if (light)
            {
                if (!settings.lampsCastShadows)
                {
                    light.shadows = LightShadows.None;
                    return;
                }
                else
                {
                    light.shadows = LightShadows.Soft;
                    light.shadowStrength = settings.lampShadowStrength;
                    light.shadowBias = settings.lampShadowBias;
                    light.shadowNormalBias = settings.lampShadowNormalBias;
                    light.shadowNearPlane = settings.lampShadowNearPlane;
                    light.shadowResolution = (UnityEngine.Rendering.LightShadowResolution)settings.lampShadowResolution;
                    light.shadowCustomResolution = -1;
                }
                light.range = settings.lampLightRange;
                light.intensity = settings.lampLightIntensity;
                light.bounceIntensity = settings.lampBounceIntensity;
                light.color = new Color(settings.lampColorR, settings.lampColorG, settings.lampColorB);
            }
        }

        [HarmonyPatch(typeof(RegionViewer), "CreateUnitGameObj")]
        static class RegionViewer_CreateUnitGameObj_Patch
        {
            static void Postfix(UnitObjInfo objInfo)
            {
                if (!enabled)
                    return;
                Singleton<TaskRunner>.Instance.RunDelayTask(0.005f, true, delegate ()
                {
                    RefreshOneLight(objInfo.go);
                });
            }
        }

        //[HarmonyPatch(typeof(LightManager), "OpenLight")]
        static class LightManager_OpenLight_Patch
        {
            static void Postfix(int groupID)
            {

                if (!enabled)
                    return;
                GameObject[] lightsObject = LightGroupData.GetLightsObject(groupID);
                for (int i = 0; i < lightsObject.Length; i++)
                {
                    RefreshOneLight(lightsObject[i]);
                }
            }
        }
    }
}
