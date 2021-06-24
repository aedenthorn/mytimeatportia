using Harmony12;
using Pathea.FarmFactoryNs;
using Pathea.UISystemNs;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityModManagerNet;

namespace FactoryStorageSize
{
    public partial class Main
    {
        public static Settings settings { get; private set; }
        public static bool enabled;
        
        private static readonly bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? typeof(Main).Namespace + " " : "") + str);
        }

        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnShowGUI = OnShowGUI;
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

        private static string storageSizeTemp;


        private static void OnShowGUI(UnityModManager.ModEntry modEntry)
        {
            storageSizeTemp = settings.FactoryStorageSize + "";
        }
        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label(string.Format("Factory Storage Size: <b>{0}</b>", settings.FactoryStorageSize), new GUILayoutOption[0]);
            settings.FactoryStorageSize = (int)GUILayout.HorizontalSlider((float)settings.FactoryStorageSize / 30, 10, 100f) * 30;
            GUILayout.Space(20f);
        }

        [HarmonyPatch(typeof(UIStateMgr), "ChangeStateByType")]
        private static class ChangeStateByType_Patch
        {
            private static void Prefix(UIStateMgr.StateType type, ref object[] objs)
            {
                if (!enabled || type != UIStateMgr.StateType.PackageExchangeState)
                    return;
                try
                {
                    if ((string)objs[1] == TextMgr.GetStr(103440, -1) && (int)objs[objs.Length - 1] == 300)
                        objs[objs.Length - 1] = settings.FactoryStorageSize;
                    Dbgl($"Setting factory storage size to {settings.FactoryStorageSize}");
                }
                catch
                {

                }

            }
        }
    }
}
