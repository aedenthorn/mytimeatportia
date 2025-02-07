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
using UnityModManagerNet;
using static Harmony12.AccessTools;

namespace QuickStore
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
            GUILayout.Label("<b>Store Key:</b>", new GUILayoutOption[0]);
            settings.StoreKey = GUILayout.TextField(settings.StoreKey, new GUILayoutOption[0]);
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

        [HarmonyPatch(typeof(PlayerItemBarCtr), "Update")]
        static class ItemBar_Patch
        {
            static void Prefix(PlayerItemBarCtr __instance)
            {
                if (!enabled)
                    return;
                if (KeyDown(settings.StoreKey) && UIStateMgr.Instance.currentState.type == UIStateMgr.StateType.Play)
                {
                    StorageUnit.SortBagToStorageAsync();
                }
            }
        }


        [HarmonyPatch(typeof(StoreageUICtr), nameof(StoreageUICtr.SortBagToStorageCheck))]
        static class StoreageUICtr_SortBagToStorageCheck_Patch
        {
            static bool Prefix(StoreageUICtr __instance)
            {
                if (!enabled || !settings.SkipConfirm)
                    return true;
                __instance.SortBagToStorage();
                return false;

            }
        }
    }
}
