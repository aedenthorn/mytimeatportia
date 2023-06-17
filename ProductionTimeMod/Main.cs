using Harmony12;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace ProductionTimeMod
{
    public partial class Main
    {
        public static bool enabled;
        private static UnityModManager.ModEntry myModEntry;

        public static Settings settings { get; private set; }

        public static void Dbgl(string str = "", bool pref = true)
        {
            Debug.Log((pref ? "ProductionTimeMod " : "") + str);
        }

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
            settings.RealTime = GUILayout.Toggle(settings.RealTime, "Randomize commerce mission level (experimental)", new GUILayoutOption[0]);
            GUILayout.Space(20);
            GUILayout.Label(string.Format("Speed Mult: <b>{0:F1}</b>", settings.SpeedMult), new GUILayoutOption[0]);
            settings.SpeedMult = (int)GUILayout.HorizontalSlider((float)settings.SpeedMult * 10, 1f, 1000f, new GUILayoutOption[0]) / 10f;
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
            catch{
                return false;
            }
        }

        static bool KeyHeld(string key)
        {
            try
            {
                return (Input.GetKey(key));
            }
            catch{
                return false;
            }
        }

    }
}
