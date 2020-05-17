using Harmony12;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace YourTime
{
    public partial class Main
    {
        public static bool enabled;
        private static UnityModManager.ModEntry myModEntry;

        public static Settings settings { get; private set; }

        // Send a response to the mod manager about the launch status, success or not.
        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            myModEntry = modEntry;
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

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label(string.Format("Time will go at {0} speed", TimeSpeedString()), new GUILayoutOption[0]);
            settings.TimeScaleModifier = GUILayout.HorizontalSlider(settings.TimeScaleModifier * 10f, 1f, 100f, new GUILayoutOption[0]) / 10f;
            GUILayout.Space(20);
            GUILayout.Label(string.Format("Key to Toggle Time"), new GUILayoutOption[0]);
            settings.StopTimeKey = GUILayout.TextField(settings.StopTimeKey, new GUILayoutOption[0]);
            GUILayout.Space(20);
            GUILayout.Label(string.Format("Key to Reduce Time Speed"), new GUILayoutOption[0]);
            settings.SlowTimeKey = GUILayout.TextField(settings.SlowTimeKey, new GUILayoutOption[0]);
            GUILayout.Space(20);
            GUILayout.Label(string.Format("Key to Increase Time Speed"), new GUILayoutOption[0]);
            settings.SpeedTimeKey = GUILayout.TextField(settings.SpeedTimeKey, new GUILayoutOption[0]);
            GUILayout.Space(20);
            GUILayout.Label(string.Format("Key to Go Back One Hour"), new GUILayoutOption[0]);
            settings.SubtractTimeKey = GUILayout.TextField(settings.SubtractTimeKey, new GUILayoutOption[0]);
            GUILayout.Space(20);
            GUILayout.Label(string.Format("Key to Go Forward One Hour"), new GUILayoutOption[0]);
            settings.AdvanceTimeKey = GUILayout.TextField(settings.AdvanceTimeKey, new GUILayoutOption[0]);
            GUILayout.Space(20);

        }

        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }

        private static string TimeSpeedString()
        {
            var speed = "normal";
            if (settings.TimeScaleModifier > 1)
            {
                speed = string.Format("{0:F1}x", settings.TimeScaleModifier);
            }
            else if (settings.TimeScaleModifier < 1)
            {
                speed = string.Format("{0:F0}/10th", settings.TimeScaleModifier*10);
            }

            return speed;
        }
    }
}
