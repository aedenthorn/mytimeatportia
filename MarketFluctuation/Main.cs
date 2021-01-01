using Harmony12;
using Pathea;
using Pathea.ConfigNs;
using Pathea.InputSolutionNs;
using Pathea.ModuleNs;
using Pathea.StoreNs;
using Pathea.TipsNs;
using System;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityModManagerNet;

namespace MarketFluctuation
{
    public partial class Main
    {
        public static bool enabled;
        private static bool isDebug = true;

        public static Settings settings { get; private set; }
        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? typeof(Main).Namespace + " " : "") + str);
        }
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

        private static bool capturingButton;
        private static float labelWidth = 80f;

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label(string.Format("Min market index: {0:F0}%", settings.MinIndex * 100), new GUILayoutOption[0]);
            settings.MinIndex = GUILayout.HorizontalSlider(settings.MinIndex * 100, 1f, 1000f, new GUILayoutOption[0]) / 100f;
            GUILayout.Space(10);
            GUILayout.Label(string.Format("Max market index: {0:F0}%", settings.MaxIndex * 100), new GUILayoutOption[0]);
            settings.MaxIndex = GUILayout.HorizontalSlider(settings.MaxIndex * 100, 1f, 1000f, new GUILayoutOption[0]) / 100f;
            GUILayout.Space(10);
            GUILayout.Label(string.Format("Min market index change per day: {0:F0}%", settings.MinIndexChange * 100), new GUILayoutOption[0]);
            settings.MinIndexChange = GUILayout.HorizontalSlider(settings.MinIndexChange * 100, 1f, 100f, new GUILayoutOption[0]) / 100f;
            GUILayout.Space(10);
            GUILayout.Label(string.Format("Max market index change per day: {0:F0}%", settings.MaxIndexChange * 100), new GUILayoutOption[0]);
            settings.MaxIndexChange = GUILayout.HorizontalSlider(settings.MaxIndexChange * 100, 1f, 100f, new GUILayoutOption[0]) / 100f;
            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Key to show current index: ", new GUILayoutOption[] { GUILayout.Width(labelWidth * 2) });
            if (!capturingButton)
            {
                settings.ShowCurrentIndex = GUILayout.TextField(settings.ShowCurrentIndex, new GUILayoutOption[] { GUILayout.Width(labelWidth * 2) });
                if (GUILayout.Button("Capture", new GUILayoutOption[]{
                        GUILayout.Width(150f)
                    }))
                {
                    capturingButton = true; 
                    GUI.FocusWindow(0);
                    GUI.FocusControl(null);
                }
            }
            else
            {
                GUILayout.TextField(settings.ShowCurrentIndex, new GUILayoutOption[] { GUILayout.Width(labelWidth * 2) });
                GUILayout.Label("Press key / button", new GUILayoutOption[] { GUILayout.Width(labelWidth * 2) });
                foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKey(kcode))
                    {
                        settings.ShowCurrentIndex = ConvertKeyCode(kcode.ToString());
                        capturingButton = false;
                        break;
                    }
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(20f);
        }
        private static string ConvertKeyCode(string str)
        {
            str = Regex.Replace(str, @"([a-z0-9])([A-Z])", @"$1 $2");
            str = Regex.Replace(str, @"([a-z])([0-9])", @"$1 $2");
            str = str.ToLower();
            str = str.Replace("keypad plus", "[+]");
            str = str.Replace("keypad minus", "[-]");
            str = str.Replace("keypad divide", "[/]");
            str = str.Replace("keypad multiply", "[*]");
            str = Regex.Replace(str, @"keypad (.*)", @"[$1]");
            str = str.Replace("alpha ", "");
            Dbgl($"capturing key: {str}");
            return str;
        }

        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }

        static bool KeyDown(string key)
        {
            try
            {
                return (Input.GetKeyDown(key));
            }
            catch
            {
                return false;
            }
        }

        [HarmonyPatch(typeof(GamingSolution), "Update")]
        static class GamingSolution_Patch
        {
            static void Postfix()
            {
                if (enabled && KeyDown(settings.ShowCurrentIndex))
                {
                    Singleton<TipsMgr>.Instance.SendImageTip(string.Format(settings.ShowMarketIndexText, Module<StoreManagerV40>.Self.CurPriceIndex * 100), MessageUITipImageAssets.ImageType.CalendarRemind, 0);
                }
            }

        }

        [HarmonyPatch(typeof(StoreManagerV40), nameof(StoreManagerV40.RefreshPriceIndex))]
        static class RefreshPriceIndex_Patch
        {
            static void Prefix()
            {
                if (enabled)
                {
                    typeof(OtherConfig).GetField("priceIndexChange", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(OtherConfig.Self, new Vector2(settings.MinIndexChange, settings.MaxIndexChange));
                    OtherConfig.Self.priceIndexLimit = new Vector2(settings.MinIndex, settings.MaxIndex);
                }
            }

        }
    }
}
