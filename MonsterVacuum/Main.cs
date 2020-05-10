using Harmony12;
using Pathea;
using Pathea.ACT;
using Pathea.ModuleNs;
using System;
using System.Collections.Generic;
using System.Reflection;
using Pathea.ActorNs;
using UnityEngine;
using UnityModManagerNet;
using static Pathea.ActorMotor;

namespace MonsterVacuum
{
    public class Main
    {
        //public static Settings settings { get; private set; }
        public static bool enabled;

        private static readonly bool isDebug = false;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "JumpRun " : "") + str);
        }

        private static void Load(UnityModManager.ModEntry modEntry)
        {
            //settings = Settings.Load<Settings>(modEntry);

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
            //settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            /*
            GUILayout.Label(string.Format("Jump height multiplier: <b>{0:F1}</b>", settings.JumpHeight), new GUILayoutOption[0]);
            settings.JumpHeight = GUILayout.HorizontalSlider(settings.JumpHeight * 10f, 10f, 50f, new GUILayoutOption[0]) / 10f;
            GUILayout.Label(string.Format("Movement Speed multiplier: <b>{0:F1}</b>", settings.MovementSpeed), new GUILayoutOption[0]);
            settings.MovementSpeed = GUILayout.HorizontalSlider(settings.MovementSpeed * 10f, 10f, 50f, new GUILayoutOption[0]) / 10f;
            settings.multiJump = GUILayout.Toggle(settings.multiJump, "Allow multi-jump", new GUILayoutOption[0]);
            */
        }
        [HarmonyPatch(typeof(Player), "Move")]
        static class Pathea_Player_Move_Patch
        {
            static void Prefix()
            {
                if (!enabled)
                    return;

            }
        }

        [HarmonyPatch(typeof(ActorMotor), "JumpStart")]
        static class Pathea_ActorMotor_MoveReal_Patch
        {
            static void Prefix(ActorMotor __instance, ref ActorMotor.JumpActionParamer ___jumpParamer)
            {
                if (!enabled)
                    return;
            }
        }
    }
}
