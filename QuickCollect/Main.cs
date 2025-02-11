using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Harmony12;
using Pathea;
using Pathea.AudioNs;
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
using static Pathea.UISystemNs.ResPath;

namespace QuickCollect
{
    public class Main
    {
        private static readonly bool isDebug = true;

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
            GUILayout.Label("<b>Collect Key:</b>", new GUILayoutOption[0]);
            settings.CollectKey = GUILayout.TextField(settings.CollectKey, new GUILayoutOption[0]);
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

            static void Prefix(PlayerItemBarCtr __instance)
            {
                if (!enabled)
                    return;

                if (KeyDown(settings.CollectKey) && UIStateMgr.Instance.currentState.type == UIStateMgr.StateType.Play)
                {
                    Dbgl($"pressed collect key");
                    foreach(var amp in GameObject.FindObjectsOfType<AutomataMachiePlace>())
                    {
                        var machine = AccessTools.FieldRefAccess<AutomataMachiePlace, AutomataMachineData>(amp, "machine");
                        if (machine is null)
                            continue;
                        List<IdCount> itemList = Module<AutomataMgr>.Self.FetchItems(machine.machineId);
                        Dbgl($"got machine {machine.machineId}, {itemList?.Count} items");
                        if (itemList == null || itemList.Count == 0)
                        {
                            continue;
                        }
                        Module<AudioModule>.Self.PlayEffect2D(17, false, true, false);
                        Module<Player>.Self.bag.AddItemList(itemList, true, AddItemMode.Default);
                        machine.ClearFinished(false);
                        AccessTools.Method(typeof(AutomataMachiePlace), "FreshItemShow", new Type[] { typeof(int) }).Invoke(amp, new object[] { machine.machineId });
                    }
                }
            }
        }
    }
}
