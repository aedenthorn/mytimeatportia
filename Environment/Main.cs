using Harmony12;
using NovaEnv;
using Pathea.WeatherNs;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;

namespace Environment
{
    public static partial class Main
    {
        private static Settings settings;
        private static bool enabled;
        private static bool isDebug = false;
        private static float labelWidth = 80f;
        private static float indentSpace = 30f;
        private static string[] toneMappers = new string[] { 
            "None",
            "ACES",
            "Neutral"
        };

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

            SceneManager.activeSceneChanged += ChangeScene;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

        }

        static void OnUpdate(UnityModManager.ModEntry modEntry, float dt)
        {
            if (Input.GetKeyDown(settings.ToggleKey))
            {
                modEntry.OnToggle(modEntry, !enabled);
                UpdateGradients();
            }
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
            UpdateGradients();
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Toggle Hotkey:"), new GUILayoutOption[] { GUILayout.Width(labelWidth * 2) });
            settings.ToggleKey = GUILayout.TextField(settings.ToggleKey, new GUILayoutOption[] { GUILayout.Width(labelWidth*2) });
            GUILayout.EndHorizontal();
            GUILayout.Space(20f);

            settings.colorsShow = GUILayout.Toggle(settings.colorsShow, $"<b>Color Variables</b>", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            if (settings.colorsShow)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(indentSpace);
                GUILayout.BeginVertical();

                SetColorSetting("SkyColor");
                SetColorSetting("FogColorA");
                SetColorSetting("FogColorB");
                SetColorSetting("UnityFogColor");
                SetColorSetting("SunBloomColor");
                SetColorSetting("SunColor", 0, 3);
                SetColorSetting("Moon1BloomColor");

                SetColorSetting("SunLightColor");

                SetColorSetting("OvercastFColor");
                SetColorSetting("OvercastBColor");
                SetColorSetting("flowCloudFColor");
                SetColorSetting("flowCloudBColor");

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            settings.gradientsShow = GUILayout.Toggle(settings.gradientsShow, $"<b>Gradient Masks</b>", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            if (settings.gradientsShow)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(indentSpace);
                GUILayout.BeginVertical();
                foreach (string g in gradientMasks)
                {
                    SetColorSetting(g + "GradientMask", -1f,1f,true);
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            settings.floatsShow = GUILayout.Toggle(settings.floatsShow, $"<b>Float Variables</b>", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            if (settings.floatsShow)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(indentSpace);
                GUILayout.BeginVertical();
                SetFloatSetting("SunColorFade",-1f);
                SetFloatSetting("FogHeight");
                SetFloatSetting("Moon1Power", 0, 100f, 1);
                SetFloatSetting("SunSize");
                SetFloatSetting("SunPower", 0, 100f, 1);
                SetFloatSetting("Overcast");
                SetFloatSetting("BloomSpread");
                SetFloatSetting("StarPower", 0, 100f, 1);

                SetFloatSetting("SunLightIntensity");

                SetFloatSetting("OvercastAlpha");
                SetFloatSetting("flowCloudAlpha");
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(20f);

        }

        private static void SetColorSetting(string name, float min = 0, float max = 1f, bool isG = false)
        {
            FieldInfo mie = typeof(Settings).GetField(name + "Enable");
            FieldInfo mif = typeof(Settings).GetField(name + "ForceMonochrome");
            FieldInfo mir = typeof(Settings).GetField(name + "R");
            FieldInfo mig = typeof(Settings).GetField(name + "G");
            FieldInfo mib = typeof(Settings).GetField(name + "B");
            FieldInfo mia = typeof(Settings).GetField(name + "A");
            FieldInfo mim = typeof(Settings).GetField(name + "ColorMonochrome");

            bool ev = (bool)mie.GetValue(settings);
            float rv = (float)mir.GetValue(settings);
            float gv = (float)mig.GetValue(settings);
            float bv = (float)mib.GetValue(settings);
            float av = (float)mia.GetValue(settings);
            float mv = 0;
            if (isG) {
                 mv = (float)mim.GetValue(settings);
            }

            GUILayout.BeginHorizontal();
            mie.SetValue(settings, GUILayout.Toggle((bool)mie.GetValue(settings), $"<b>{name}</b>", new GUILayoutOption[0]));
            if (isG)
            {
                mif.SetValue(settings, GUILayout.Toggle((bool)mif.GetValue(settings), $"<b>Force Monochrome</b>", new GUILayoutOption[] { GUILayout.Width(labelWidth*2f) }));
                
            }
            GUILayout.EndHorizontal();

            if ((bool)mie.GetValue(settings))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(indentSpace);
                GUILayout.BeginVertical();
                if (isG && (bool)mif.GetValue(settings))
                {
                    GUILayout.Label(string.Format("Grey: <b>{0:F2}</b> ", mim.GetValue(settings)), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
                    mim.SetValue(settings, GUILayout.HorizontalSlider((float)mim.GetValue(settings) * 100f, 100f * min, 100f * max) / 100f);
                }
                else
                {
                    GUILayout.Label(string.Format("Red: <b>{0:F2}</b> ", mir.GetValue(settings)), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
                    mir.SetValue(settings, GUILayout.HorizontalSlider((float)mir.GetValue(settings) * 100f, 100f * min, 100f * max) / 100f);
                    GUILayout.Label(string.Format("Green: <b>{0:F2}</b> ", mig.GetValue(settings)), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
                    mig.SetValue(settings, GUILayout.HorizontalSlider((float)mig.GetValue(settings) * 100f, 100f * min, 100f * max) / 100f);
                    GUILayout.Label(string.Format("Blue: <b>{0:F2}</b> ", mib.GetValue(settings)), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
                    mib.SetValue(settings, GUILayout.HorizontalSlider((float)mib.GetValue(settings) * 100f, 100f * min, 100f * max) / 100f);
                }
                GUILayout.Label(string.Format("Alpha: <b>{0:F2}</b> ", mia.GetValue(settings)), new GUILayoutOption[] { GUILayout.Width(labelWidth) });
                mia.SetValue(settings, GUILayout.HorizontalSlider((float)mia.GetValue(settings) * 100f, 100f * min, 100f * max) / 100f);
                if (isG && defaultGradients != null)
                {
                    List<string> defColorStrings = new List<string>();
                    WeatherCtr weatherCtr = (WeatherCtr)typeof(WeatherModule).GetField("weatherCtr", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(WeatherModule.Self);
                    Executor executor = (Executor)typeof(WeatherCtr).GetField("executor", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(weatherCtr);
                    GradientColorKey[] defColors = defaultGradients[executor.BiomoIndex][name.Replace("GradientMask", "")].colorKeys;
                    foreach (GradientColorKey gc in defColors)
                    {
                        defColorStrings.Add($"({Math.Round(gc.color.r*100)/100f},{Math.Round(gc.color.g * 100) / 100f},{Math.Round(gc.color.b * 100) / 100f},{Math.Round(gc.color.a * 100) / 100f})");
                    }
                    GUILayout.Label("Defaults: " + string.Join(",", defColorStrings.ToArray()), new GUILayoutOption[0]);
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            GUILayout.Space(20f);

            if (isG && (ev != (bool)mie.GetValue(settings) || rv != (float)mir.GetValue(settings) || gv != (float)mig.GetValue(settings) || bv != (float)mib.GetValue(settings) || av != (float)mia.GetValue(settings) || mv != (float)mim.GetValue(settings)))
            {
                UpdateGradients();
            }

        }

        private static void SetFloatSetting(string name, float min = 0f, float max = 1f, float scale = 100f)
        {
            FieldInfo mie = typeof(Settings).GetField(name + "Enable");
            FieldInfo miv = typeof(Settings).GetField(name);


            mie.SetValue(settings, GUILayout.Toggle((bool)mie.GetValue(settings), $"<b>{name}</b>", new GUILayoutOption[0]));

            if ((bool)mie.GetValue(settings))
            {
                miv.SetValue(settings, GUILayout.HorizontalSlider((float)miv.GetValue(settings) * scale, scale*min, scale*max) / scale);
            }
            GUILayout.Space(20f);
        }

        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }
        private static void ChangeScene(Scene arg0, Scene arg1)
        {
            if(arg1.name == "Main")
            {
                UpdateGradients();
            }
        }


    }
}
