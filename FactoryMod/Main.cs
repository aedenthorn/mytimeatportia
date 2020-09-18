using Harmony12;
using Pathea;
using Pathea.AnimalFarmNs;
using Pathea.HomeViewerNs;
using Pathea.MailNs;
using Pathea.ModuleNs;
using Pathea.UISystemNs;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace FactoryMod
{
    public class Main
    {
        public static Settings settings { get; private set; }

        public static bool enabled;

        private static readonly bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "FactoryMod " : "") + str);
        }

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
        }

        [HarmonyPatch(typeof(FBM_BuildingSelect), nameof(FBM_BuildingSelect.InitAllBuilding))]
        static class FBM_BuildingSelect_InitAllBuilding
        {
            static void Prefix()
            {
                if (!Module<FarmBuildingModeModule>.Self.IsBuildingActive(FarmBuildingEnum.Factory))
                {
                    Module<FarmBuildingModeModule>.Self.SetBuildingActive(FarmBuildingEnum.Factory, true);
                }

            }
        }
    }
}
