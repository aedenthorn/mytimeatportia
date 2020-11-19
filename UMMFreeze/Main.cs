using Harmony12;
using Pathea;
using Pathea.GameFlagNs;
using Pathea.UISystemNs;
using System;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace UMMFreeze
{
    public class Main
    {
        public static bool enabled;

        private static readonly bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "UMMFreeze " : "") + str);
        }

        private static void Load(UnityModManager.ModEntry modEntry)
        {

            modEntry.OnToggle = OnToggle;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }


        public static BoolTrue MouseLocker { get; set; } = new BoolTrue();

        [HarmonyPatch(typeof(UnityModManager.UI), "BlockGameUI")]
        static class UnityModManager_UI_BlockGameUI
        {

            static void Prefix(bool value)
            {
                if (!enabled)
                    return;

                try
                {
                    if (value)
                    {
                        Dbgl("Opening UMM");
                        Dbgl("Freezing UI");

                        UIStateComm.Instance.SetCursor(true);
                        Singleton<GameFlag>.Instance.Add(Flag.Pause, MouseLocker);
                        Dbgl("UI Frozen");
                    }
                    else
                    {
                        Dbgl("Closing UMM");
                        Dbgl("Unfreezing UI");
                        UIStateComm.Instance.SetCursor(false);
                        Singleton<GameFlag>.Instance.Remove(Flag.Pause, MouseLocker);
                        Dbgl("UI Unfrozen");
                    }
                }
                catch(Exception ex)
                {
                    Dbgl("Exception: "+ex);
                }
            }
        }

        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }

    }
}
