using System;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;
using Harmony12;
using Pathea.Missions;
using Pathea.GuildRanking;

namespace FasterPiggy
{
    public static class Main
    {
        public static bool enabled;

        public static Settings settings { get; private set; }

        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label(string.Format("Machine Pig speed multiplier: <b>{0:F1}</b>", settings.MachinePigSpeedMult), new GUILayoutOption[0]);
            settings.MachinePigSpeedMult = GUILayout.HorizontalSlider(settings.MachinePigSpeedMult, 1f, 3f, new GUILayoutOption[0]);
            settings.MachinePigSpeedMult = (float)Math.Round((double)settings.MachinePigSpeedMult, 1);
            GUILayout.Label(string.Format("Machine Pig lift speed multiplier: <b>{0:F1}</b>", settings.MachinePigLiftMult), new GUILayoutOption[0]);
            settings.MachinePigLiftMult = GUILayout.HorizontalSlider(settings.MachinePigLiftMult, 1f, 5f, new GUILayoutOption[0]);
            settings.MachinePigLiftMult = (float)Math.Round((double)settings.MachinePigLiftMult, 1);
            GUILayout.Label(string.Format("Machine Pig stamina consumption: <b>{00:F0}%</b>", settings.MachinePigConsumePercent), new GUILayoutOption[0]);
            settings.MachinePigConsumePercent = (int)GUILayout.HorizontalSlider((float)settings.MachinePigConsumePercent, 1f, 100f, new GUILayoutOption[0]);
        }

        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }
        [HarmonyPatch(typeof(Pathea.RiderNs.MachineFlyPig), "Run", new Type[] {typeof(float) })]
        static class Pathea_RiderNs_MachineFlyPig_Run_Patch
        {
            static bool Prefix(ref float ___noPowerMoveSpeed, ref float ___maxMoveSpeed, ref float ___pullUpSpeed, ref float ___pullUpCdConsumeSpeed)
            {
                if (!Main.enabled)
                    return true;
                ___noPowerMoveSpeed = 15f * settings.MachinePigSpeedMult;
                ___maxMoveSpeed = 30f * settings.MachinePigSpeedMult;
                ___pullUpSpeed = 10f * settings.MachinePigLiftMult;
                ___pullUpCdConsumeSpeed = 20f * settings.MachinePigConsumePercent / 100;
                return true;
            }
        }
        [HarmonyPatch(typeof(Pathea.RiderNs.MachineFlyPig), "PlayRunSound", new Type[] { })]
        static class Pathea_RiderNs_MachineFlyPig_PlayRunSound_Patch
        {
            static bool Prefix(bool ___inInput, bool ___inPullUp)
            {
                if (!Main.enabled)
                    return true;
                if (___inInput | ___inPullUp)
                    return true;
                return false;
            }
        }
    }
}