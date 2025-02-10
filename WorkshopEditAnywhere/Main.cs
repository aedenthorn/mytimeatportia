using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Harmony12;
using Pathea;
using Pathea.FarmFactoryNs;
using Pathea.HomeNs;
using Pathea.HomeViewerNs;
using Pathea.InputSolutionNs;
using Pathea.ItemSystem;
using Pathea.MessageSystem;
using Pathea.ModuleNs;
using Pathea.SpawnNs;
using Pathea.UISystemNs;
using Pathea.UISystemNs.UIBase;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;
using static Harmony12.AccessTools;

namespace WorkshopEditAnywhere
{
    public class Main
    {
        private static readonly bool isDebug = false;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log($"{(pref ? typeof(Main).Namespace : "") } {str}");
        }
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
            GUILayout.Label("<b>Menu Key:</b>", new GUILayoutOption[0]);
            settings.MenuKey = GUILayout.TextField(settings.MenuKey, new GUILayoutOption[0]);
            GUILayout.Space(20);
        }
        static bool KeyDown(string key)
        {
            try
            {
                return (Input.GetKeyDown(key));
            }
            catch
            {
                return false;
            }
        }

        private static bool isSorting;

        [HarmonyPatch(typeof(PlayerItemBarCtr), "Update")]
        static class PlayerItemBarCtr_Update_Patch
        {

            static void Prefix()
            {
                if (!enabled)
                    return;
                if (KeyDown(settings.MenuKey) && UIStateMgr.Instance.currentState.type == UIStateMgr.StateType.Play)
                {
                    if (SceneManager.GetActiveScene().name != "Main")
                    {
                        Dbgl("Not in main scene!");
                        return;
                    }
                    UIStateMgr.Instance.ChangeStateByType(UIStateMgr.StateType.FarmBuilding, false, new object[] { null });

                }
            }
        }

    }
}
