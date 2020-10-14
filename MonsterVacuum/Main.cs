using Harmony12;
using Pathea;
using Pathea.MessageSystem;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;

namespace MonsterVacuum
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
                Debug.Log((pref ? "MonsterVacuum " : "") + str);
        }

        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            vacuumed = new List<int>();
            MessageManager.Instance.Subscribe("WakeUpScreenEnd", new Action<object[]>(OnWakeUp));
            SceneManager.activeSceneChanged += ChangeScene;

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnToggle = OnToggle;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private static void ChangeScene(Scene arg0, Scene arg1)
        {
            vacuumed.Clear();
        }

        private static void OnWakeUp(object[] obj)
        {
            vacuumed.Clear();
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
            GUILayout.Label(string.Format("Vacuum Range: <b>{0}</b>", settings.VacuumRadius), new GUILayoutOption[0]);
            settings.VacuumRadius = (int)GUILayout.HorizontalSlider(settings.VacuumRadius, 1f, 1000f, new GUILayoutOption[0]);
        }
        
        //[HarmonyPatch(typeof(Player), "Move")]
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
