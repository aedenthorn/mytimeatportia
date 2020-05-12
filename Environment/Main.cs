using Harmony12;
using NovaEnv;
using Pathea;
using Pathea.ModuleNs;
using Pathea.WeatherNs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityModManagerNet;

namespace Environment
{
    public class Main
    {
        private static Settings settings;
        private static bool enabled;
        private static bool isDebug = true;
        private static float buttonWidth = 20f;
        private static float labelWidth = 80f;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                UnityEngine.Debug.Log((pref ? "Environment " : "") + str);
        }
        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            modEntry.OnUpdate = OnUpdate;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

        }
        static void OnUpdate(UnityModManager.ModEntry modEntry, float dt)
        {
            if (Input.GetKeyDown(settings.ToggleKey))
            {
                modEntry.OnToggle(modEntry, !enabled);
            }
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Toggle Hotkey:"), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.ToggleKey = GUILayout.TextField(settings.ToggleKey, new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            GUILayout.EndHorizontal();
            GUILayout.Space(20f);

            GUILayout.BeginHorizontal();
            settings.SkyColorEnable = GUILayout.Toggle(settings.SkyColorEnable,"", new GUILayoutOption[] { GUILayout.Width(buttonWidth) });
            GUILayout.Label("<b>SkyColor</b>");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Red: <b>{0:F2}</b> ",settings.SkyColorR), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.SkyColorR = GUILayout.HorizontalSlider(settings.SkyColorR*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Green: <b>{0:F2}</b> ",settings.SkyColorG), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.SkyColorG = GUILayout.HorizontalSlider(settings.SkyColorG*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Blue: <b>{0:F2}</b> ",settings.SkyColorB), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.SkyColorB = GUILayout.HorizontalSlider(settings.SkyColorB*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Alpha: <b>{0:F2}</b> ",settings.SkyColorA), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.SkyColorA = GUILayout.HorizontalSlider(settings.SkyColorA*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.Space(20f);

            GUILayout.BeginHorizontal();
            settings.FogColorAEnable = GUILayout.Toggle(settings.FogColorAEnable,"", new GUILayoutOption[] { GUILayout.Width(buttonWidth) });
            GUILayout.Label("<b>FogColorA</b>");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Red: <b>{0:F2}</b> ",settings.FogColorAR), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.FogColorAR = GUILayout.HorizontalSlider(settings.FogColorAR*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Green: <b>{0:F2}</b> ",settings.FogColorAG), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.FogColorAG = GUILayout.HorizontalSlider(settings.FogColorAG*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Blue: <b>{0:F2}</b> ",settings.FogColorAB), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.FogColorAB = GUILayout.HorizontalSlider(settings.FogColorAB*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Alpha: <b>{0:F2}</b> ",settings.FogColorAA), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.FogColorAA = GUILayout.HorizontalSlider(settings.FogColorAA * 100f, 0, 100f) / 100f;
            GUILayout.EndHorizontal();
            GUILayout.Space(20f);

            GUILayout.BeginHorizontal();
            settings.FogColorBEnable = GUILayout.Toggle(settings.FogColorBEnable,"", new GUILayoutOption[] { GUILayout.Width(buttonWidth) });
            GUILayout.Label("<b>FogColorB</b>");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Red: <b>{0:F2}</b> ",settings.FogColorBR), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.FogColorBR = GUILayout.HorizontalSlider(settings.FogColorBR*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Green: <b>{0:F2}</b> ",settings.FogColorBG), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.FogColorBG = GUILayout.HorizontalSlider(settings.FogColorBG*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Blue: <b>{0:F2}</b> ",settings.FogColorBB), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.FogColorBB = GUILayout.HorizontalSlider(settings.FogColorBB*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Alpha: <b>{0:F2}</b> ",settings.FogColorBA), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.FogColorBA = GUILayout.HorizontalSlider(settings.FogColorBA * 100f, 0, 100f) / 100f;
            GUILayout.EndHorizontal();
            GUILayout.Space(20f);

            GUILayout.BeginHorizontal();
            settings.UnityFogColorEnable = GUILayout.Toggle(settings.UnityFogColorEnable,"", new GUILayoutOption[] { GUILayout.Width(buttonWidth) });
            GUILayout.Label("<b>UnityFogColor</b>");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Red: <b>{0:F2}</b> ",settings.UnityFogColorR), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.UnityFogColorR = GUILayout.HorizontalSlider(settings.UnityFogColorR*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Green: <b>{0:F2}</b> ",settings.UnityFogColorG), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.UnityFogColorG = GUILayout.HorizontalSlider(settings.UnityFogColorG*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Blue: <b>{0:F2}</b> ",settings.UnityFogColorB), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.UnityFogColorB = GUILayout.HorizontalSlider(settings.UnityFogColorB*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Alpha: <b>{0:F2}</b> ",settings.UnityFogColorA), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.UnityFogColorA = GUILayout.HorizontalSlider(settings.UnityFogColorA * 100f, 0, 100f) / 100f;
            GUILayout.EndHorizontal();
            GUILayout.Space(20f);

            GUILayout.BeginHorizontal();
            settings.SunBloomColorEnable = GUILayout.Toggle(settings.SunBloomColorEnable,"", new GUILayoutOption[] { GUILayout.Width(buttonWidth) });
            GUILayout.Label("<b>SunBloomColor</b>");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Red: <b>{0:F2}</b> ",settings.SunBloomColorR), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.SunBloomColorR = GUILayout.HorizontalSlider(settings.SunBloomColorR*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Green: <b>{0:F2}</b> ",settings.SunBloomColorG), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.SunBloomColorG = GUILayout.HorizontalSlider(settings.SunBloomColorG*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Blue: <b>{0:F2}</b> ",settings.SunBloomColorB), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.SunBloomColorB = GUILayout.HorizontalSlider(settings.SunBloomColorB*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Alpha: <b>{0:F2}</b> ",settings.SunBloomColorA), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.SunBloomColorA = GUILayout.HorizontalSlider(settings.SunBloomColorA * 100f, 0, 100f) / 100f;
            GUILayout.EndHorizontal();
            GUILayout.Space(20f);

            GUILayout.BeginHorizontal();
            settings.SunColorEnable = GUILayout.Toggle(settings.SunColorEnable,"", new GUILayoutOption[] { GUILayout.Width(buttonWidth) });
            GUILayout.Label("<b>SunColor</b>");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Red: <b>{0:F2}</b> ",settings.SunColorR), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.SunColorR = GUILayout.HorizontalSlider(settings.SunColorR*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Green: <b>{0:F2}</b> ",settings.SunColorG), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.SunColorG = GUILayout.HorizontalSlider(settings.SunColorG*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Blue: <b>{0:F2}</b> ",settings.SunColorB), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.SunColorB = GUILayout.HorizontalSlider(settings.SunColorB*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Alpha: <b>{0:F2}</b> ",settings.SunColorA), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.SunColorA = GUILayout.HorizontalSlider(settings.SunColorA * 100f, 0, 100f) / 100f;
            GUILayout.EndHorizontal();
            GUILayout.Space(20f);

            GUILayout.BeginHorizontal();
            settings.Moon1BloomColorEnable = GUILayout.Toggle(settings.Moon1BloomColorEnable,"", new GUILayoutOption[] { GUILayout.Width(buttonWidth) });
            GUILayout.Label("<b>Moon1BloomColor</b>");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Red: <b>{0:F2}</b> ",settings.Moon1BloomColorR), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.Moon1BloomColorR = GUILayout.HorizontalSlider(settings.Moon1BloomColorR*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Green: <b>{0:F2}</b> ",settings.Moon1BloomColorG), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.Moon1BloomColorG = GUILayout.HorizontalSlider(settings.Moon1BloomColorG*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Blue: <b>{0:F2}</b> ",settings.Moon1BloomColorB), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.Moon1BloomColorB = GUILayout.HorizontalSlider(settings.Moon1BloomColorB*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Alpha: <b>{0:F2}</b> ",settings.Moon1BloomColorA), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.Moon1BloomColorA = GUILayout.HorizontalSlider(settings.Moon1BloomColorA*100f, 0, 100f) / 100f;
            GUILayout.EndHorizontal();
            GUILayout.Space(20f);

            GUILayout.BeginHorizontal();
            settings.SunLightColorEnable = GUILayout.Toggle(settings.SunLightColorEnable,"", new GUILayoutOption[] { GUILayout.Width(buttonWidth) });
            GUILayout.Label("<b>SunLightColor</b>");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Red: <b>{0:F2}</b> ",settings.SunLightColorR), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.SunLightColorR = GUILayout.HorizontalSlider(settings.SunLightColorR*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Green: <b>{0:F2}</b> ",settings.SunLightColorG), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.SunLightColorG = GUILayout.HorizontalSlider(settings.SunLightColorG*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Blue: <b>{0:F2}</b> ",settings.SunLightColorB), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.SunLightColorB = GUILayout.HorizontalSlider(settings.SunLightColorB*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Alpha: <b>{0:F2}</b> ",settings.SunLightColorA), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.SunLightColorA = GUILayout.HorizontalSlider(settings.SunLightColorA*100f, 0, 100f) / 100f;
            GUILayout.EndHorizontal();
            GUILayout.Space(20f);

            GUILayout.BeginHorizontal();
            settings.OvercastFColorEnable = GUILayout.Toggle(settings.OvercastFColorEnable,"", new GUILayoutOption[] { GUILayout.Width(buttonWidth) });
            GUILayout.Label("<b>OvercastFColor</b>");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Red: <b>{0:F2}</b> ",settings.OvercastFColorR), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.OvercastFColorR = GUILayout.HorizontalSlider(settings.OvercastFColorR*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Green: <b>{0:F2}</b> ",settings.OvercastFColorG), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.OvercastFColorG = GUILayout.HorizontalSlider(settings.OvercastFColorG*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Blue: <b>{0:F2}</b> ",settings.OvercastFColorB), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.OvercastFColorB = GUILayout.HorizontalSlider(settings.OvercastFColorB*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Alpha: <b>{0:F2}</b> ",settings.OvercastFColorA), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.OvercastFColorA = GUILayout.HorizontalSlider(settings.OvercastFColorA*100f, 0, 100f) / 100f;
            GUILayout.EndHorizontal();
            GUILayout.Space(20f);

            GUILayout.BeginHorizontal();
            settings.OvercastBColorEnable = GUILayout.Toggle(settings.OvercastBColorEnable,"", new GUILayoutOption[] { GUILayout.Width(buttonWidth) });
            GUILayout.Label("<b>OvercastBColor</b>");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Red: <b>{0:F2}</b> ",settings.OvercastBColorR), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.OvercastBColorR = GUILayout.HorizontalSlider(settings.OvercastBColorR*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Green: <b>{0:F2}</b> ",settings.OvercastBColorG), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.OvercastBColorG = GUILayout.HorizontalSlider(settings.OvercastBColorG*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Blue: <b>{0:F2}</b> ",settings.OvercastBColorB), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.OvercastBColorB = GUILayout.HorizontalSlider(settings.OvercastBColorB*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Alpha: <b>{0:F2}</b> ",settings.OvercastBColorA), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.OvercastBColorA = GUILayout.HorizontalSlider(settings.OvercastBColorA*100f, 0, 100f) / 100f;
            GUILayout.EndHorizontal();
            GUILayout.Space(20f);

            GUILayout.BeginHorizontal();
            settings.flowCloudFColorEnable = GUILayout.Toggle(settings.flowCloudFColorEnable,"", new GUILayoutOption[] { GUILayout.Width(buttonWidth) });
            GUILayout.Label("<b>flowCloudFColor</b>");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Red: <b>{0:F2}</b> ",settings.flowCloudFColorR), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.flowCloudFColorR = GUILayout.HorizontalSlider(settings.flowCloudFColorR*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Green: <b>{0:F2}</b> ",settings.flowCloudFColorG), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.flowCloudFColorG = GUILayout.HorizontalSlider(settings.flowCloudFColorG*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Blue: <b>{0:F2}</b> ",settings.flowCloudFColorB), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.flowCloudFColorB = GUILayout.HorizontalSlider(settings.flowCloudFColorB*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Alpha: <b>{0:F2}</b> ",settings.flowCloudFColorA), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.flowCloudFColorA = GUILayout.HorizontalSlider(settings.flowCloudFColorA*100f, 0, 100f) / 100f;
            GUILayout.EndHorizontal();
            GUILayout.Space(20f);

            GUILayout.BeginHorizontal();
            settings.flowCloudBColorEnable = GUILayout.Toggle(settings.flowCloudBColorEnable,"", new GUILayoutOption[] { GUILayout.Width(buttonWidth) });
            GUILayout.Label("<b>flowCloudBColor</b>");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Red: <b>{0:F2}</b> ",settings.flowCloudBColorR), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.flowCloudBColorR = GUILayout.HorizontalSlider(settings.flowCloudBColorR*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Green: <b>{0:F2}</b> ",settings.flowCloudBColorG), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.flowCloudBColorG = GUILayout.HorizontalSlider(settings.flowCloudBColorG*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Blue: <b>{0:F2}</b> ",settings.flowCloudBColorB), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.flowCloudBColorB = GUILayout.HorizontalSlider(settings.flowCloudBColorB*100f,0,100f)/100f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Alpha: <b>{0:F2}</b> ",settings.flowCloudBColorA), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
            settings.flowCloudBColorA = GUILayout.HorizontalSlider(settings.flowCloudBColorA*100f, 0, 100f) / 100f;
            GUILayout.EndHorizontal();
            GUILayout.Space(20f);


            GUILayout.BeginHorizontal();
            settings.SunColorFadeEnable = GUILayout.Toggle(settings.SunColorFadeEnable,"", new GUILayoutOption[] { GUILayout.Width(buttonWidth) });
            GUILayout.Label(string.Format("<b>SunColorFade</b>: {0:F2}", settings.SunColorFade), new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            settings.SunColorFade = GUILayout.HorizontalSlider(settings.SunColorFade * 100f, 0, 100f) / 100f;
            GUILayout.Space(10f);
            GUILayout.BeginHorizontal();
            settings.FogHeightEnable = GUILayout.Toggle(settings.FogHeightEnable,"", new GUILayoutOption[] { GUILayout.Width(buttonWidth) });
            GUILayout.Label(string.Format("<b>FogHeight</b>: {0:F2}", settings.FogHeight), new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            settings.FogHeight = GUILayout.HorizontalSlider(settings.FogHeight * 100f, 0, 100f) / 100f;
            GUILayout.Space(10f);
            GUILayout.BeginHorizontal();
            settings.Moon1PowerEnable = GUILayout.Toggle(settings.Moon1PowerEnable,"", new GUILayoutOption[] { GUILayout.Width(buttonWidth) });
            GUILayout.Label(string.Format("<b>Moon1Power</b>: {0:F0}", settings.Moon1Power), new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            settings.Moon1Power = GUILayout.HorizontalSlider(settings.Moon1Power, 0, 100f);
            GUILayout.Space(10f);
            GUILayout.BeginHorizontal();
            settings.SunSizeEnable = GUILayout.Toggle(settings.SunSizeEnable,"", new GUILayoutOption[] { GUILayout.Width(buttonWidth) });
            GUILayout.Label(string.Format("<b>SunSize</b>: {0:F2}", settings.SunSize), new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            settings.SunSize = GUILayout.HorizontalSlider(settings.SunSize * 100f, 0, 100f) / 100f;
            GUILayout.Space(10f);
            GUILayout.BeginHorizontal();
            settings.SunPowerEnable = GUILayout.Toggle(settings.SunPowerEnable,"", new GUILayoutOption[] { GUILayout.Width(buttonWidth) });
            GUILayout.Label(string.Format("<b>SunPower</b>: {0:F0}", settings.SunPower), new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            settings.SunPower = GUILayout.HorizontalSlider(settings.SunPower, 0, 100f);
            GUILayout.Space(10f);
            GUILayout.BeginHorizontal();
            settings.OvercastEnable = GUILayout.Toggle(settings.OvercastEnable,"", new GUILayoutOption[] { GUILayout.Width(buttonWidth) });
            GUILayout.Label(string.Format("<b>Overcast</b>: {0:F2}", settings.Overcast), new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            settings.Overcast = GUILayout.HorizontalSlider(settings.Overcast * 100f, 0, 100f) / 100f;
            GUILayout.Space(10f);
            GUILayout.BeginHorizontal();
            settings.BloomSpreadEnable = GUILayout.Toggle(settings.BloomSpreadEnable,"", new GUILayoutOption[] { GUILayout.Width(buttonWidth) });
            GUILayout.Label(string.Format("<b>BloomSpread</b>: {0:F2}", settings.BloomSpread), new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            settings.BloomSpread = GUILayout.HorizontalSlider(settings.BloomSpread * 100f, 0, 100f) / 100f;
            GUILayout.Space(10f);
            GUILayout.BeginHorizontal();
            settings.StarPowerEnable = GUILayout.Toggle(settings.StarPowerEnable,"", new GUILayoutOption[] { GUILayout.Width(buttonWidth) });
            GUILayout.Label(string.Format("<b>StarPower</b>: {0:F2}", settings.StarPower), new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            settings.StarPower = GUILayout.HorizontalSlider(settings.StarPower, 0, 100f);
            GUILayout.Space(10f);
            
            GUILayout.BeginHorizontal();
            settings.SunLightIntensityEnable = GUILayout.Toggle(settings.SunLightIntensityEnable,"", new GUILayoutOption[] { GUILayout.Width(buttonWidth) });
            GUILayout.Label(string.Format("<b>SunLightIntensity</b>: {0:F2}", settings.SunLightIntensity), new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            settings.SunLightIntensity = GUILayout.HorizontalSlider(settings.SunLightIntensity*100f, 0, 100f)/100f;
            GUILayout.Space(10f);

            GUILayout.BeginHorizontal();
            settings.OvercastAlphaEnable = GUILayout.Toggle(settings.OvercastAlphaEnable,"", new GUILayoutOption[] { GUILayout.Width(buttonWidth) });
            GUILayout.Label(string.Format("<b>OvercastAlpha</b>: {0:F2}", settings.OvercastAlpha), new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            settings.OvercastAlpha = GUILayout.HorizontalSlider(settings.OvercastAlpha*100f, 0, 100f)/100f;
            GUILayout.Space(10f);
            GUILayout.BeginHorizontal();
            settings.flowCloudAlphaEnable = GUILayout.Toggle(settings.flowCloudAlphaEnable,"", new GUILayoutOption[] { GUILayout.Width(buttonWidth) });
            GUILayout.Label(string.Format("<b>flowCloudAlpha</b>: {0:F2}", settings.flowCloudAlpha), new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            settings.flowCloudAlpha = GUILayout.HorizontalSlider(settings.flowCloudAlpha*100f, 0, 100f)/100f;

        }

        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }

        [HarmonyPatch(typeof(Output), "Apply")]
        static class Output_Patch
        {
            static void Prefix(ref Output __instance)
            {
                if (!enabled)
                    return;

                Dbgl("SunLightColor " + __instance.SunLightColor);
                Dbgl("SunLightIntensity " + __instance.SunLightIntensity);

                Dbgl("overcastFColor " + __instance.overcastFColor);
                Dbgl("overcastBColor " + __instance.overcastBColor);
                Dbgl("flowCloudFColor " + __instance.flowCloudFColor);
                Dbgl("flowCloudBColor " + __instance.flowCloudBColor);
                Dbgl("overcastAlpha " + __instance.OvercastAlpha);
                Dbgl("flowCloudAlpha " + __instance.flowCloudAlpha);


                if (settings.SunLightColorEnable)
                    __instance.SunLightColor = new Color(settings.SunLightColorR, settings.SunLightColorG, settings.SunLightColorB, settings.SunLightColorA);
                if (settings.SunLightIntensityEnable)
                    __instance.SunLightIntensity = settings.SunLightIntensity;

                if (settings.OvercastFColorEnable)
                    __instance.overcastFColor = new Color(settings.OvercastFColorR, settings.OvercastFColorG, settings.OvercastFColorB, settings.OvercastFColorA);
                if(settings.OvercastBColorEnable)
                    __instance.overcastBColor = new Color(settings.OvercastBColorR, settings.OvercastBColorG, settings.OvercastBColorB, settings.OvercastBColorA);
                if(settings.flowCloudFColorEnable)
                    __instance.flowCloudFColor = new Color(settings.flowCloudFColorR, settings.flowCloudFColorG, settings.flowCloudFColorB, settings.flowCloudFColorA);
                if(settings.flowCloudBColorEnable)
                    __instance.flowCloudBColor = new Color(settings.flowCloudBColorR, settings.flowCloudBColorG, settings.flowCloudBColorB, settings.flowCloudBColorA);

                if (settings.SunLightColorEnable)
                    __instance.OvercastAlpha = settings.OvercastAlpha;
                if (settings.SunLightColorEnable)
                    __instance.flowCloudAlpha = settings.flowCloudAlpha;

            }
        }
        [HarmonyPatch(typeof(Sky), "Apply")]
        static class Sky_Patch
        {
            static void Prefix(ref Sky __instance)
            {
                if (!enabled)
                    return;
                /*
                Dbgl("SkyColor"+__instance.SkyColor);
                Dbgl("FogColorA"+__instance.FogColorA);
                Dbgl("FogColorB"+__instance.FogColorB);
                Dbgl("UnityFogColor"+__instance.UnityFogColor);
                Dbgl("SunBloomColor"+__instance.SunBloomColor);
                Dbgl("SunColor"+__instance.SunColor);
                Dbgl("Moon1BloomColor"+__instance.Moon1BloomColor);
                Dbgl("FogHeight"+__instance.FogHeight);
                Dbgl("SunColorFade"+__instance.SunColorFade);
                Dbgl("Moon1Power"+__instance.Moon1Power);
                Dbgl("SunSize"+__instance.SunSize);
                Dbgl("SunPower"+__instance.SunPower);
                Dbgl("Overcast"+__instance.Overcast);
                Dbgl("BloomSpread"+__instance.BloomSpread);
                Dbgl("StarPower"+__instance.StarPower);
                */

                if(settings.SkyColorEnable)
					__instance.SkyColor = new Color(settings.SkyColorR, settings.SkyColorG, settings.SkyColorB, settings.SkyColorA); 
                if(settings.FogColorAEnable)
					__instance.FogColorA = new Color(settings.FogColorAR, settings.FogColorAG, settings.FogColorAB, settings.FogColorAA); 
                if(settings.FogColorBEnable)
					__instance.FogColorB = new Color(settings.FogColorBR, settings.FogColorBG, settings.FogColorBB, settings.FogColorBA); 
                if(settings.UnityFogColorEnable)
					__instance.UnityFogColor = new Color(settings.UnityFogColorR, settings.UnityFogColorG, settings.UnityFogColorB, settings.UnityFogColorA); 
                if(settings.SunBloomColorEnable)
					__instance.SunBloomColor = new Color(settings.SunBloomColorR, settings.SunBloomColorG, settings.SunBloomColorB, settings.SunBloomColorA); 
                if(settings.SunColorEnable)
					__instance.SunColor = new Color(settings.SunColorR, settings.SunColorG, settings.SunColorB, settings.SunColorA); 
                if(settings.Moon1BloomColorEnable)
					__instance.Moon1BloomColor = new Color(settings.Moon1BloomColorR, settings.Moon1BloomColorG, settings.Moon1BloomColorB, settings.Moon1BloomColorA); 
                if(settings.FogHeightEnable)
					__instance.FogHeight = settings.FogHeight;
                if(settings.SunColorFadeEnable)
					__instance.SunColorFade = settings.SunColorFade;
                if(settings.Moon1PowerEnable)
					__instance.Moon1Power = settings.Moon1Power;
                if(settings.SunSizeEnable)
					__instance.SunSize = settings.SunSize;
                if(settings.SunPowerEnable)
					__instance.SunPower = settings.SunPower;
                if(settings.OvercastEnable)
					__instance.Overcast = settings.Overcast;
                if(settings.BloomSpreadEnable)
					__instance.BloomSpread = settings.BloomSpread;
                if(settings.StarPowerEnable)
					__instance.StarPower = settings.StarPower;
            }
        }
    }
}
