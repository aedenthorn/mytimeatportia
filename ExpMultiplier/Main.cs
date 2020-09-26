using Harmony12;
using Pathea;
using System;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace ExpMultiplier
{
    public class Main
    {

        public static bool enabled;

        public static Settings settings { get; private set; }

        private static readonly bool isDebug = false;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "ExpMultiplier " : "") + str);
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
            GUILayout.Label(string.Format("Experience Multiplier : <b>{0:F1}x</b>", settings.ExpMult), new GUILayoutOption[0]);
            settings.ExpMult = GUILayout.HorizontalSlider((float)Main.settings.ExpMult * 10f, 1f, 1000f, new GUILayoutOption[0]) / 10f;
            GUILayout.Space(20);
        }

        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }

        [HarmonyPatch(typeof(Player), "AddExp")]
        public static class MapIconInteractTransfer_GetQuestion
        {
            public static void Prefix(ref int exp)
            {
                if (!enabled)
                    return;

                exp = (int)Math.Round(exp * settings.ExpMult);
            }
        }
    }
}
