using Ccc;
using Harmony12;
using Pathea;
using Pathea.AudioNs;
using Pathea.CameraSystemNs;
using Pathea.GameFlagNs;
using Pathea.MessageSystem;
using Pathea.ModuleNs;
using Pathea.ScenarioNs;
using Pathea.UISystemNs;
using Pathea.UISystemNs.CustomActor;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;

namespace CustomizePlayer
{
    public static class Main
    {
        public static bool enabled;
        private static bool isCustomizing = false;
        private static bool isDebug = true;
        private static Vector3 lastPosition;
        private static Vector3 lastRotation;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "CustomizePlayer " : "") + str);
        }
        public static Settings settings { get; private set; }

        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnUpdate = OnUpdate;
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            SceneManager.activeSceneChanged += ChangeScene;

        }

        private static void ChangeScene(Scene arg0, Scene arg1)
        {
            if(arg1.name != "custom" && arg1.name != "Game")
            {
                isCustomizing = false;
            }
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (GUILayout.Button("Customize", new GUILayoutOption[]{
                GUILayout.Width(150f)
            }))
            {
                BeginCustomizing();

                UnityModManager.UI.Instance.ToggleWindow();
            }
            GUILayout.Label(string.Format("Customize Hotkey:"), new GUILayoutOption[0]);
            settings.OpenCustomizeKey = GUILayout.TextField(settings.OpenCustomizeKey, new GUILayoutOption[0]);
        }


        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value; 
            return true; // Permit or not.
        }

        static void OnUpdate(UnityModManager.ModEntry modEntry, float dt)
        {
            if (Input.GetKeyDown(settings.OpenCustomizeKey) && Module<Player>.Self.actor != null && Singleton<GameFlag>.Instance.Gaming)
            {
                BeginCustomizing();

            }
        }

        private static void BeginCustomizing()
        {
            isCustomizing = true;
            lastScenario = Module<ScenarioModule>.Self.CurrentScenarioName;
            lastPosition = Module<Player>.Self.actor.gamePos;
            lastRotation = Module<Player>.Self.actor.gameRot.eulerAngles;

            string ls = "custom";


            Module<ScenarioModule>.Self.LoadScenario(new PortalData
            {
                fromScenario = ls,
                toScenario = "custom",
                disableJsonData = false,
            }, null);

            MessageManager.Instance.Dispatch("PlayerEditUI", new object[]
            {
                    true
            }, DispatchType.IMME, 2f);
        }

        private static string lastScenario;

        [HarmonyPatch(typeof(CustomActorUI), "SetSex")]
        static class SetSex_Patch
        {
            static bool Prefix()
            {
                if (!enabled || !isCustomizing)
                    return true;

                return false;
            }
        }

        [HarmonyPatch(typeof(CustomActorUI), "Confirm")]
        static class Confirm_Patch
        {
            static bool Prefix(CustomActorUI __instance)
            {

                if (!enabled || !isCustomizing)
                    return true;

                Module<AudioModule>.Self.PlayEffect2D(68, false, true, false);
                if (__instance.GreenBar(__instance.nameInput.text))
                {
                    UIUtils.ShowTipsMode2(TextMgr.GetStr(100501, -1), Color.white, 1.5f, null, false);
                }
                else if (Module<GameMgr>.Self.CheckTooManyCharacters())
                {
                    UIUtils.ShowTipsMode2(TextMgr.GetStr(100901, -1), Color.white, 1.5f, null, false);
                }
                else
                {
                    __instance.gameObject.SetActive(false);
                    CalendarUIStdState.ShowBirthdaySelect(new Action(ReturnToGame), delegate
                    {
                        __instance.gameObject.SetActive(true);
                    });
                }
                return false;
            }

        }
        private static void ReturnToGame()
        {
            isCustomizing = false;
            UIStateMgr.Instance.PopState(false);
            Module<ScenarioModule>.Self.TransferToScenario(lastScenario, lastPosition, lastRotation);
        }
    }
}