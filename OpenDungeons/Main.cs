using Harmony12;
using Pathea;
using Pathea.CameraSystemNs;
using Pathea.ModuleNs;
using Pathea.ScenarioNs;
using PatheaScriptExt;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace OpenDungeons
{
    public class Main
    {
        //public static Settings settings { get; private set; }
        public static bool enabled;

        private static readonly bool isDebug = false;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "OpenDungeons " : "") + str);
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

        //[HarmonyPatch(typeof(ScenarioModule), "PostLoad")]
        static class ScenarioModule_PostLoad_Patch
        {
            static void Postfix()
            {
                Module<SceneItemManager>.Self.SetItemInteract((SceneItemType)1, 106, true);
                Module<SceneItemManager>.Self.Create((SceneItemType)0, 13640, "Mission/Mission_Arrow2", "Main", new Vector3(765.61f, 97.97f, 74.11f), new Vector3(0, 0, 0), "", false, AssetType.Mission);
                Module<SceneItemManager>.Self.Create((SceneItemType)0, 136401, "Mission/Mission_Arrow2", "TestRandom", new Vector3(386.4505f, 30.98072f, 402.6f), new Vector3(0, 0, 0), "", false, AssetType.Mission);
            }
        }
    }
}
