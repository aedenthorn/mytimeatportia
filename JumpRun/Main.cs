using Harmony12;
using Pathea;
using Pathea.ACT;
using Pathea.ModuleNs;
using System;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace JumpRun
{
    public class Main
    {
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
            GUILayout.Label(string.Format("Jump height multiplier: <b>{0:F1}</b>", settings.JumpHeight), new GUILayoutOption[0]);
            settings.JumpHeight = GUILayout.HorizontalSlider(settings.JumpHeight, 1f, 5f, new GUILayoutOption[0]);
            settings.JumpHeight = (float)Math.Round((double)settings.JumpHeight, 1);
            GUILayout.Label(string.Format("Movement Speed multiplier: <b>{0:F1}</b>", settings.MovementSpeed), new GUILayoutOption[0]);
            settings.MovementSpeed = GUILayout.HorizontalSlider(settings.MovementSpeed, 1f, 5f, new GUILayoutOption[0]);
            settings.MovementSpeed = (float)Math.Round((double)settings.MovementSpeed, 1);
        }
        [HarmonyPatch(typeof(Player), "Move")]
        static class Pathea_Player_Move_Patch
        {
            static void Prefix()
            {
                if (!enabled)
                    return;
                Module<Player>.Self.actor.motor.maxSpeed = 16f*settings.MovementSpeed;
                Module<Player>.Self.actor.RunSpeed = 7.2f* settings.MovementSpeed;
                Module<Player>.Self.actor.FastRunSpeed = 16f*settings.MovementSpeed;
            }
        }
        [HarmonyPatch(typeof(ACTJump), "CanDo")]
        static class Pathea_ACTJump_CanDo_Patch
        {
            static bool Prefix(ref bool __result)
            {
                if (!enabled)
                    return true;
                __result = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(ActorMotor), "MoveReal")]
        static class Pathea_ActorMotor_JumpStart_Patch
        {
            static void Prefix(ref ActorMotor.JumpActionParamer ___jumpParamer)
            {
                if (!enabled)
                    return;
                if (___jumpParamer != null)
                {
                    ___jumpParamer.Gravity = 80f / settings.JumpHeight;
                }
            }
        }
    }
}
