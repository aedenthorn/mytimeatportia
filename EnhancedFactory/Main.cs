using Harmony12;
using Pathea;
using Pathea.HomeViewerNs;
using Pathea.MessageSystem;
using Pathea.ModuleNs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityModManagerNet;

namespace EnhancedFactory
{
    public class Main
    {
        private static readonly bool isDebug = true;
        private static Settings settings;
        private static bool enabled = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "DatabaseExtension " : "") + str);
        }
        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnToggle = OnToggle;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            MessageManager.Instance.Subscribe("WakeUpScreenEnd", new Action<object[]>(OnWakeUp));

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
        }
        private static void OnWakeUp(object[] obj)
        {
            if (!Module<FarmBuildingModeModule>.Self.IsBuildingActive(FarmBuildingEnum.Factory))
            {
               //Module<FarmBuildingModeModule>.Self.SetBuildingActive(FarmBuildingEnum.Factory, true);
            }
        }
    }
}
