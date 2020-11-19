using Harmony12;
using Pathea;
using Pathea.HomeNs;
using Pathea.HomeViewerNs;
using Pathea.LightMgr;
using Pathea.ModuleNs;
using Pathea.ScenarioNs;
using Pathea.SwitchNs;
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
                RefreshAllLights(SceneManager.GetActiveScene());
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
                RefreshAllLights(SceneManager.GetActiveScene());
            }
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            settings.showWorkshopSettings = GUILayout.Toggle(settings.showWorkshopSettings, "Workshop Light Settings", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            if (settings.showWorkshopSettings)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(40f);
                GUILayout.BeginVertical();
                GUILayout.Label(string.Format("Workshop Lamp Light Range: <b>{0:F1}</b>", settings.lampLightRange), new GUILayoutOption[0]);
                settings.lampLightRange = GUILayout.HorizontalSlider(settings.lampLightRange * 10f, 0f, 10000f, new GUILayoutOption[0]) / 10f;
                GUILayout.Space(10f);
                GUILayout.Label(string.Format("Workshop Lamp Light Intensity: <b>{0:F2}</b>", settings.lampLightIntensity), new GUILayoutOption[0]);
                settings.lampLightIntensity = GUILayout.HorizontalSlider(settings.lampLightIntensity * 100f, 0f, 1000f, new GUILayoutOption[0]) / 100f;
                GUILayout.Space(10f);
                GUILayout.Label(string.Format("Workshop Lamp Bounce Intensity: <b>{0:F2}</b>", settings.lampBounceIntensity), new GUILayoutOption[0]);
                settings.lampBounceIntensity = GUILayout.HorizontalSlider(settings.lampBounceIntensity * 100f, 0f, 1000f, new GUILayoutOption[0]) / 100f;
                GUILayout.Space(10f);
                GUILayout.Label(string.Format("Workshop Lamp Light Color"), new GUILayoutOption[0]);
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

                settings.lampsCastShadows = GUILayout.Toggle(settings.lampsCastShadows, "Workshop Lamps Cast Shadows", new GUILayoutOption[0]);
                GUILayout.Space(10f);
                if (settings.lampsCastShadows)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(40f);
                    GUILayout.BeginVertical();

                    GUILayout.Label(string.Format("Workshop Lamp Shadow Strength: <b>{0:F2}</b>", settings.lampShadowStrength), new GUILayoutOption[0]);
                    settings.lampShadowStrength = GUILayout.HorizontalSlider(settings.lampShadowStrength * 100f, 0f, 100f, new GUILayoutOption[0]) / 100f;
                    GUILayout.Space(10f);
                    GUILayout.Label(string.Format("Workshop Lamp Shadow Near Plane: <b>{0:F1}</b>", settings.lampShadowNearPlane), new GUILayoutOption[0]);
                    settings.lampShadowNearPlane = GUILayout.HorizontalSlider(settings.lampShadowNearPlane * 10f, 1f, 100f, new GUILayoutOption[0]) / 10f;
                    GUILayout.Space(10f);
                    GUILayout.Label(string.Format("Workshop Lamp Shadow Bias: <b>{0:F2}</b>", settings.lampShadowBias), new GUILayoutOption[0]);
                    settings.lampShadowBias = GUILayout.HorizontalSlider(settings.lampShadowBias * 100f, 0f, 200f, new GUILayoutOption[0]) / 100f;
                    GUILayout.Space(10f);
                    GUILayout.Label(string.Format("Workshop Lamp Shadow Normal Bias: <b>{0:F2}</b>", settings.lampShadowNormalBias), new GUILayoutOption[0]);
                    settings.lampShadowNormalBias = GUILayout.HorizontalSlider(settings.lampShadowNormalBias * 100f, 0f, 300f, new GUILayoutOption[0]) / 100f;
                    GUILayout.Space(10f);
                    GUILayout.Label(string.Format("Workshop Lamp Shadow Resolution: <b>{0}</b>", shadowResolutions[settings.lampShadowResolution + 1]), new GUILayoutOption[0]);
                    settings.lampShadowResolution = (int)GUILayout.HorizontalSlider(settings.lampShadowResolution, -1f, 3f, new GUILayoutOption[0]);
                    GUILayout.Space(10f);

                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();

                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

            }


            GUILayout.Space(10f);

            settings.showSceneSettings = GUILayout.Toggle(settings.showSceneSettings, "Scene Light Settings", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            if (settings.showSceneSettings)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(40f);
                GUILayout.BeginVertical();


                settings.customStreetlightLights = GUILayout.Toggle(settings.customStreetlightLights, "Customize Scene Lights", new GUILayoutOption[0]);
                GUILayout.Space(10f);
                if (settings.customStreetlightLights)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(40f);
                    GUILayout.BeginVertical();

                    GUILayout.Label(string.Format("Scene Light Light Range Mult: <b>{0:F1}</b>", settings.streetlightLightRangeMult), new GUILayoutOption[0]);
                    settings.streetlightLightRangeMult = GUILayout.HorizontalSlider(settings.streetlightLightRangeMult * 100f, -100f, 100f, new GUILayoutOption[0]) / 100f;
                    GUILayout.Space(10f);
                    GUILayout.Label(string.Format("Scene Light Light Intensity Mult: <b>{0:F2}</b>", settings.streetlightLightIntensityMult), new GUILayoutOption[0]);
                    settings.streetlightLightIntensityMult = GUILayout.HorizontalSlider(settings.streetlightLightIntensityMult * 100f, -100f, 100f, new GUILayoutOption[0]) / 100f;
                    GUILayout.Space(10f);
                    GUILayout.Label(string.Format("Scene Light Bounce Intensity Mult: <b>{0:F2}</b>", settings.streetlightBounceIntensityMult), new GUILayoutOption[0]);
                    settings.streetlightBounceIntensityMult = GUILayout.HorizontalSlider(settings.streetlightBounceIntensityMult * 100f, -100f, 100f, new GUILayoutOption[0]) / 100f;
                    GUILayout.Space(10f);
                    GUILayout.Label(string.Format("Scene Light Light Color"), new GUILayoutOption[0]);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(40f);
                    GUILayout.BeginVertical();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(string.Format("Red Mult: <b>{0:F2}</b> ", settings.streetlightColorRMult), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
                    settings.streetlightColorRMult = GUILayout.HorizontalSlider((float)settings.streetlightColorRMult * 100f, -100f, 100f) / 100f;
                    GUILayout.EndHorizontal();
                    GUILayout.Space(10f);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(string.Format("Green Mult: <b>{0:F2}</b> ", settings.streetlightColorGMult), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
                    settings.streetlightColorGMult = GUILayout.HorizontalSlider((float)settings.streetlightColorGMult * 100f, -100f, 100f) / 100f;
                    GUILayout.EndHorizontal();
                    GUILayout.Space(10f);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(string.Format("Blue Mult: <b>{0:F2}</b> ", settings.streetlightColorBMult), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
                    settings.streetlightColorBMult = GUILayout.HorizontalSlider((float)settings.streetlightColorBMult * 100f, -100f, 100f) / 100f;
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();

                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();

                }
                GUILayout.Space(20f);

                settings.streetlightsCastShadows = GUILayout.Toggle(settings.streetlightsCastShadows, "Scene Lights Cast Shadows", new GUILayoutOption[0]);
                GUILayout.Space(10f);
                if (settings.streetlightsCastShadows)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(40f);
                    GUILayout.BeginVertical();

                    GUILayout.Label(string.Format("Scene Light Shadow Strength: <b>{0:F2}</b>", settings.streetlightShadowStrength), new GUILayoutOption[0]);
                    settings.streetlightShadowStrength = GUILayout.HorizontalSlider(settings.streetlightShadowStrength * 100f, 0f, 100f, new GUILayoutOption[0]) / 100f;
                    GUILayout.Space(10f);
                    GUILayout.Label(string.Format("Scene Light Shadow Near Plane: <b>{0:F1}</b>", settings.streetlightShadowNearPlane), new GUILayoutOption[0]);
                    settings.streetlightShadowNearPlane = GUILayout.HorizontalSlider(settings.streetlightShadowNearPlane * 10f, 1f, 100f, new GUILayoutOption[0]) / 10f;
                    GUILayout.Space(10f);
                    GUILayout.Label(string.Format("Scene Light Shadow Bias: <b>{0:F2}</b>", settings.streetlightShadowBias), new GUILayoutOption[0]);
                    settings.streetlightShadowBias = GUILayout.HorizontalSlider(settings.streetlightShadowBias * 100f, 0f, 200f, new GUILayoutOption[0]) / 100f;
                    GUILayout.Space(10f);
                    GUILayout.Label(string.Format("Scene Light Shadow Normal Bias: <b>{0:F2}</b>", settings.streetlightShadowNormalBias), new GUILayoutOption[0]);
                    settings.streetlightShadowNormalBias = GUILayout.HorizontalSlider(settings.streetlightShadowNormalBias * 100f, 0f, 300f, new GUILayoutOption[0]) / 100f;
                    GUILayout.Space(10f);
                    GUILayout.Label(string.Format("Scene Light Shadow Resolution: <b>{0}</b>", shadowResolutions[settings.streetlightShadowResolution + 1]), new GUILayoutOption[0]);
                    settings.streetlightShadowResolution = (int)GUILayout.HorizontalSlider(settings.streetlightShadowResolution, -1f, 3f, new GUILayoutOption[0]);
                    GUILayout.Space(10f);

                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }

        private static void ChangeScene(Scene arg0, Scene arg1)
        {
            if (!enabled || arg1.name != "Main")
                return;
            //origLights.Clear();
            RefreshAllLights(arg1);
        }

        private static void RefreshAllLights(Scene scene)
        {
            Dbgl($"Refreshing Lights");
            LayeredRegion layeredRegion = (LayeredRegion)typeof(FarmModule).GetField("layeredRegion", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Module<FarmModule>.Self);
            if(layeredRegion == null)
            {
                Dbgl("layeredRegion  is null");
                return;
            }
            Region itemLayer = (Region)typeof(LayeredRegion).GetField("itemLayer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(layeredRegion);
            if (itemLayer == null)
            {
                Dbgl("itemLayer  is null");
                return;
            }
            List<object> slots = AccessTools.FieldRefAccess<Region, List<object>>(itemLayer, "slots");
            if (slots == null)
            {
                Dbgl("slots is null");
                return;
            }
            if (settings.customStreetlightLights)
            {
                GameObject[] gameObjects = scene.GetRootGameObjects();
                foreach (GameObject obj in gameObjects)
                {
                    RefreshOneStreetLight(obj);
                }
            }
            foreach (object slot in slots)
            {
                UnitObjInfo unitObjInfo = (UnitObjInfo)slot?.GetType().GetField("unitObjInfo").GetValue(slot);
                GameObject go = unitObjInfo?.go;
                if(go?.GetComponentInChildren<Light>() != null)
                {
                    RefreshOneWorkshopLight(go);
                }
            }
        }

        private static void RefreshOneWorkshopLight(GameObject go)
        {
            if (go == null)
                return;

            Light light = go.GetComponentInChildren<Light>();
            if (light)
            {
                if (!settings.lampsCastShadows)
                {
                    light.shadows = LightShadows.None;
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

        private static Dictionary<int, MyLight> origLights = new Dictionary<int, MyLight>();

        private static void RefreshOneStreetLight(GameObject go)
        {
            if (go == null)
                return;

            Light[] lights = go.GetComponentsInChildren<Light>();
            foreach(Light light in lights)
            {
                if (!settings.streetlightsCastShadows)
                {
                    light.shadows = LightShadows.None;
                    return;
                }
                else
                {
                    light.shadows = LightShadows.Soft;
                    light.shadowStrength = settings.streetlightShadowStrength;
                    light.shadowBias = settings.streetlightShadowBias;
                    light.shadowNormalBias = settings.streetlightShadowNormalBias;
                    light.shadowNearPlane = settings.streetlightShadowNearPlane;
                    light.shadowResolution = (UnityEngine.Rendering.LightShadowResolution)settings.streetlightShadowResolution;
                    light.shadowCustomResolution = -1;
                }
                if (!origLights.ContainsKey(go.GetInstanceID()))
                    origLights.Add(go.GetInstanceID(), new MyLight(light));
                MyLight origLight = origLights[go.GetInstanceID()];
                light.range = origLight.range + (settings.streetlightLightRangeMult < 0 ? settings.streetlightLightRangeMult * origLight.range : (1000f - origLight.range)*settings.streetlightLightRangeMult);
                light.intensity = origLight.intensity + (settings.streetlightLightIntensityMult < 0 ? settings.streetlightLightIntensityMult * origLight.intensity : (10f - origLight.intensity) * settings.streetlightLightIntensityMult);
                light.bounceIntensity = origLight.bounceIntensity + (settings.streetlightBounceIntensityMult < 0 ? settings.streetlightBounceIntensityMult * origLight.bounceIntensity: (10f - origLight.bounceIntensity) * settings.streetlightBounceIntensityMult);
                float r = origLight.color.r + (settings.streetlightColorRMult < 0 ? settings.streetlightColorRMult * origLight.color.r : (1f - origLight.color.r) * settings.streetlightColorRMult);
                float g = origLight.color.g + (settings.streetlightColorGMult < 0 ? settings.streetlightColorGMult * origLight.color.g : (1f - origLight.color.g) * settings.streetlightColorGMult);
                float b = origLight.color.b + (settings.streetlightColorBMult < 0 ? settings.streetlightColorBMult * origLight.color.b : (1f - origLight.color.b) * settings.streetlightColorBMult);
                light.color = new Color(r,g,b, origLight.color.a);
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
                    RefreshOneWorkshopLight(objInfo.go);
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
                    RefreshOneWorkshopLight(lightsObject[i]);
                }
            }
        }
    }
}
