using Harmony12;
using Pathea.CameraSystemNs;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace NoWiggle
{
    public class Main
    {
        //public static Settings settings { get; private set; }
        public static bool enabled;

        private static readonly bool isDebug = false;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "NoWiggle " : "") + str);
        }

        private static void Load(UnityModManager.ModEntry modEntry)
        {
            //settings = Settings.Load<Settings>(modEntry);

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnToggle = OnToggle;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }
        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            //settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
        }
        [HarmonyPatch(typeof(CameraWiggle), "ExecuteWiggle")]
        static class Wiggle_Patch
        {
            static bool Prefix()
            {
                if(enabled)
                    return false;

                return true;
            }
        }
    }
}
