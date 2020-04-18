using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony12;
using Pathea.Missions;
using Pathea.ModuleNs;
using UnityEngine;
using UnityModManagerNet;

namespace MissionDebug
{
    public class Main
    {
        public static Settings settings { get; private set; }
        public static bool enabled;
        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

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
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (GUILayout.Button("Dee Dee Board", new GUILayoutOption[]{
                GUILayout.Width(150f)
            }))
            {
                Module<MissionManager>.Self.DeliverMultiContentMission(1100302, 5, 0, "0", 0, "0");
                Module<MissionManager>.Self.AddMissionToSceneItem(1100302, 7, 12052);
            }
        }
    }
}
