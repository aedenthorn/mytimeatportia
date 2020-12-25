using Harmony12;
using Pathea;
using Pathea.ActorNs;
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

namespace Regeneration
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
            harmony.Patch(
               original: AccessTools.Method(typeof(Player), "Update"),
               postfix: new HarmonyMethod(typeof(Main), nameof(Player_Update_Postfix))
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

        private static float lastHealthTick = 0;
        private static float lastStaminaTick = 0;

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label(string.Format("Health Regen Rate (seconds for one tick): <b>{0:F1}</b>", settings.HealthRegen), new GUILayoutOption[0]);
            settings.HealthRegen = GUILayout.HorizontalSlider(settings.HealthRegen * 10f, 1f, 100f, new GUILayoutOption[0]) / 10f;
            GUILayout.Label(string.Format("Stamina Regen Rate (seconds for one tick): <b>{0:F1}</b>", settings.StaminaRegen), new GUILayoutOption[0]);
            settings.StaminaRegen = GUILayout.HorizontalSlider(settings.StaminaRegen * 10f, 1f, 100f, new GUILayoutOption[0]) / 10f;
            GUILayout.Space(10);
            settings.RegenWhenStopped = GUILayout.Toggle(settings.RegenWhenStopped, "Regen while time is stopped", new GUILayoutOption[0]);
            GUILayout.Space(20);
        }

        private static void Player_Update_Postfix(Player __instance, Actor ___playingActor)
        {
            if (!enabled || ___playingActor == null || (!settings.RegenWhenStopped && Module<TimeManager>.Self.TimeStoped))
                return;

            if (Time.fixedTime > lastHealthTick + settings.HealthRegen)
            {
                __instance.ChangeHp(1);
                lastHealthTick = Time.fixedTime;
            }
            if (Time.fixedTime > lastStaminaTick + settings.StaminaRegen)
            {
                __instance.actor.cp += 1;
                lastStaminaTick = Time.fixedTime;
            }
        }

    }
}
