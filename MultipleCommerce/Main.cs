using System.Reflection;
using UnityEngine;
using UnityModManagerNet;
using Harmony12;

namespace MultipleCommerce
{
    public static partial class Main
    {
        private static bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref?"Multiple Commerce ":"") + str);
        }
        public static bool enabled;

        public static Settings settings { get; private set; }

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
            GUILayout.Label(string.Format("Maximum total number of commerce orders available per day: <b>{0:F0}</b>", settings.NumberCommerceOrders), new GUILayoutOption[0]);
            settings.NumberCommerceOrders = (int)GUILayout.HorizontalSlider((float)Main.settings.NumberCommerceOrders, 1f, 20f, new GUILayoutOption[0]);
            GUILayout.Label(string.Format("Maximum number of <b>big</b> commerce orders available per day: <b>{0:F0}</b>", settings.NumberBigOrders), new GUILayoutOption[0]);
            settings.NumberBigOrders = (int)GUILayout.HorizontalSlider((float)Main.settings.NumberBigOrders, 1f, 10f, new GUILayoutOption[0]);
            GUILayout.Label(string.Format("Maximum number of <b>special</b> commerce missions available per day: <b>{0:F0}</b>", settings.NumberSpecialOrders), new GUILayoutOption[0]);
            settings.NumberSpecialOrders = (int)GUILayout.HorizontalSlider((float)Main.settings.NumberSpecialOrders, 1f, 10f, new GUILayoutOption[0]);
        }

        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }
    }
}