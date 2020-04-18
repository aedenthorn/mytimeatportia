using Harmony12;
using Pathea;
using Pathea.ACT;
using Pathea.ActorNs;
using Pathea.BlackBoardNs;
using Pathea.FavorSystemNs;
using Pathea.MessageSystem;
using Pathea.MG;
using Pathea.ModuleNs;
using Pathea.NpcRepositoryNs;
using PatheaScriptExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;

namespace TestMod
{
    public partial class Main
    {
        public static bool enabled;
        public static readonly int PennyID = 4000141;
        // Send a response to the mod manager about the launch status, success or not.
        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            return true;
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
        }
        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }
        [HarmonyPatch(typeof(MGMgr), "GetMarriageID")]
        static class MGMgr_GetMarriageID_Patch
        {
            static void Prefix()
            {
                Debug.Log("XYZ XYZ XYZ");
            }
        }
    }
}