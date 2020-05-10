using Harmony12;
using Pathea;
using Pathea.GameFlagNs;
using Pathea.MessageSystem;
using Pathea.ModuleNs;
using Pathea.ScenarioNs;
using System;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace CustomizePlayer
{
    public static class Main
    {
        public static bool enabled;

        public static Settings settings { get; private set; }

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

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (GUILayout.Button("Customize", new GUILayoutOption[]{
                GUILayout.Width(150f)
            }))
            {
                Module<ScenarioModule>.Self.LoadScenario("custom", 0, false, null);
                UnityModManager.UI.Instance.ToggleWindow();
            }
            GUILayout.Label(string.Format("Customize Hotkey:"), new GUILayoutOption[0]);
            settings.OpenCustomizeKey = GUILayout.TextField(settings.OpenCustomizeKey, new GUILayoutOption[0]);
        }

        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value; 
            return true; // Permit or not.
        }

        static void OnUpdate(UnityModManager.ModEntry modEntry, float dt)
        {
            if (Input.GetKeyDown(settings.OpenCustomizeKey) && Module<Player>.Self.actor != null && Singleton<GameFlag>.Instance.Gaming)
            {
                Module<ScenarioModule>.Self.LoadScenario("custom", 0, false, null);
                MessageManager.Instance.Dispatch("PlayerEditUI", new object[]
                {
                    true
                }, DispatchType.IMME, 2f);
            }
        }

    }
}