using Harmony12;
using Pathea;
using Pathea.CompoundSystem;
using Pathea.ConfigNs;
using Pathea.FeatureNs;
using Pathea.Missions;
using Pathea.ModuleNs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityModManagerNet;

namespace FactoryWorktableDiscount
{
    public partial class Main
    {
        public static Settings settings { get; private set; }
        public static bool enabled;
        
        public static List<int> vacuumed;

        private static readonly bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "FactoryWorktableDiscount " : "") + str);
        }

        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnToggle = OnToggle;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            harmony.Patch(
               original: AccessTools.Method(typeof(AutomataMachineMenuCtr), "SetItemData"),
               prefix: new HarmonyMethod(typeof(Main), nameof(SetIsWorkTable_Prefix)),
               postfix: new HarmonyMethod(typeof(Main), nameof(SetIsWorkTable_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(AutomataMachineMenuCtr), "FreshInfo"),
               prefix: new HarmonyMethod(typeof(Main), nameof(SetIsWorkTable_Prefix)),
               postfix: new HarmonyMethod(typeof(Main), nameof(SetIsWorkTable_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(AutomataMachineMenuCtr), "StartAutomataResult"),
               prefix: new HarmonyMethod(typeof(Main), nameof(SetIsWorkTable_Prefix)),
               postfix: new HarmonyMethod(typeof(Main), nameof(SetIsWorkTable_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(AutomataMachineMenuCtr), nameof(AutomataMachineMenuCtr.StartAutomata)),
               prefix: new HarmonyMethod(typeof(Main), nameof(SetIsWorkTable_Prefix)),
               postfix: new HarmonyMethod(typeof(Main), nameof(SetIsWorkTable_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(AutomataMachineMenuCtr), nameof(AutomataMachineMenuCtr.CancelResult)),
               prefix: new HarmonyMethod(typeof(Main), nameof(SetIsWorkTable_Prefix)),
               postfix: new HarmonyMethod(typeof(Main), nameof(SetIsWorkTable_Postfix))
            );


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

            //GUILayout.Label(string.Format("Maximum Displayed Missions (requires game restart): <b>{0}</b>", settings.MaxMissions), new GUILayoutOption[0]);
            //settings.MaxMissions = (int)GUILayout.HorizontalSlider(settings.MaxMissions, 1f, 30f, new GUILayoutOption[0]);
        }


        public static void SetIsWorkTable_Prefix(ref bool __state, ref bool ___isWorktable)
        {
            if (!enabled || ___isWorktable)
                return;

            __state = true;
            ___isWorktable = true;
        }
        public static void SetIsWorkTable_Postfix(bool __state, ref bool ___isWorktable)
        {
            if (!enabled)
                return;

            if (__state)
            {
                ___isWorktable = false;
            }
        }

    }
}
