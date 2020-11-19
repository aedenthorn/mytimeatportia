using Harmony12;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;

namespace CustomTextures
{
    public partial class Main
    {

        private static bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "Custom Textures " : "") + str);
        }
        public static bool enabled;

        public static Settings settings { get; private set; }

        private static void Load(UnityModManager.ModEntry modEntry)
        {

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            settings = Settings.Load<Settings>(modEntry);

            LoadCustomTextures();
            SceneManager.activeSceneChanged += ChangeScene;
        }


        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (GUILayout.Button("Reload Textures", new GUILayoutOption[]{
                GUILayout.Width(250f),
                GUILayout.Height(80f),
            }))
            {
                ReloadTextures();
            }
            if (GUILayout.Button("Dump Scene Names", new GUILayoutOption[]{
                GUILayout.Width(250f),
                GUILayout.Height(80f)
            }))
            {
                DumpObjectNames();
                UnityModManager.OpenUnityFileLog();
            }
        }


        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }
        

        private static void ChangeScene(Scene arg0, Scene arg1)
        {
            if (!enabled)
            {
                return;
            }
            FixSceneTextures(arg1);
            ReloadActorTextures();
        }

    }
}
