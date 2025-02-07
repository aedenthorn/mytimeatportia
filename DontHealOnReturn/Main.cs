using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Harmony12;
using Pathea;
using Pathea.Behavior;
using Pathea.BuffNs;
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
using PatheaScriptExt;
using UnityEngine;
using UnityModManagerNet;
using static Harmony12.AccessTools;

namespace DontHealOnReturn
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

            //modEntry.OnGUI = OnGUI;
           // modEntry.OnSaveGUI = OnSaveGUI;
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

        [HarmonyPatch(typeof(Return), nameof(Return.OnStart))]
        static class Return_OnStart_Patch
        {
            static void Postfix(Return __instance, BuffItem ____buff)
            {
                if (!enabled)
                    return;
                __instance.actor.Value.Buff.RemoveBuff(____buff);

            }
        }

    }
}
