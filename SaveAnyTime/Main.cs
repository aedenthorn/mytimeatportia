using Harmony12;
using Pathea;
using Pathea.InputSolutionNs;
using Pathea.MessageSystem;
using Pathea.ModuleNs;
using Pathea.PlayerMissionNs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;

namespace SaveAnyTime
{
    public partial class Main
    {
        private static bool isDebug = true;

        private static List<CustomSaveFile> saveFiles = new List<CustomSaveFile>();
        private static float lastSave = -1f;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "SaveAnyTime " : "") + str);
        }
        public static bool enabled;
        private static bool isLoading = false;
        private static bool isSaving = false;
        private static bool gameLoaded = false;

        public static Settings settings { get; private set; }

        // Send a response to the mod manager about the launch status, success or not.
        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnShowGUI = OnShowGUI;
            modEntry.OnUpdate = OnUpdate;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            DoBuildSaveList();
            SceneManager.activeSceneChanged += ChangeScene;
            MessageManager.Instance.Subscribe("WakeUpScreenEnd", new Action<object[]>(OnWakeUp));
            return true;
        }

        private static void ChangeScene(Scene arg0, Scene arg1)
        {
            if(arg1.name != "Game" && settings.saveOnSceneChange)
            {
                if (!gameLoaded)
                {
                    gameLoaded = true;
                }
                else
                {
                    DoSaveFile(true);
                }
            }
        }

        private static void OnWakeUp(object[] obj)
        {
            resetLastSave();
        }

        private static void resetLastSave()
        {
            lastSave = Time.fixedTime;
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label("Quick Save Key:", new GUILayoutOption[0]);
            settings.QuickSaveKey = GUILayout.TextField(settings.QuickSaveKey, new GUILayoutOption[0]);
            GUILayout.Space(10f);
            GUILayout.Label("Quick Load Key:", new GUILayoutOption[0]);
            settings.QuickLoadKey = GUILayout.TextField(settings.QuickLoadKey, new GUILayoutOption[0]);
            GUILayout.Space(10f);
            GUILayout.Label(settings.saveInterval == 0 ? "Autosave Interval: <b>off</b>" : string.Format("Autosave Interval: <b>every {0:F0} minutes</b> (0 to turn off)", settings.saveInterval), new GUILayoutOption[0]);
            int saveInterval = (int)GUILayout.HorizontalSlider(settings.saveInterval, 0f, 120f, new GUILayoutOption[0]);
            if(settings.saveInterval != saveInterval)
            {
                resetLastSave();
                settings.saveInterval = saveInterval;
            }

            GUILayout.Space(10f);

            GUILayout.Label(string.Format("Max auto saves: <b>{0}</b>", settings.maxAutoSaves), new GUILayoutOption[0]);
            settings.maxAutoSaves = (int)GUILayout.HorizontalSlider(settings.maxAutoSaves, 1f, 20f, new GUILayoutOption[0]);

            GUILayout.Space(10f);
            settings.saveOnSceneChange = GUILayout.Toggle(settings.saveOnSceneChange, "Autosave when changing scenes", new GUILayoutOption[0]);

            if (Player.Self != null && Module<Player>.Self != null && Module<Player>.Self.actor != null)
            {
                GUILayout.Space(20f);
                if (GUILayout.Button("Save Now", new GUILayoutOption[]{
                        GUILayout.Width(150f)
                    }))
                {
                    DoSaveFile();
                }
            }

            if (saveFiles.Count > 0)
            {
                GUILayout.Space(20f);
                GUILayout.Label("Load Game", new GUILayoutOption[0]);
                try
                {
                    foreach (CustomSaveFile csf in saveFiles)
                    {
                        GUILayout.BeginHorizontal(new GUILayoutOption[0]);
                        if (GUILayout.Button(csf.saveTitle, new GUILayoutOption[0]))
                        {
                            Singleton<TaskRunner>.Self.StartCoroutine(LoadGameFromArchive(csf.fileName));
                            UnityModManager.UI.Instance.ToggleWindow();
                        }
                        if (GUILayout.Button("X", new GUILayoutOption[]{GUILayout.Width(50f)}))
                        {
                            DeleteSaveGame(csf.fileName);
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                catch
                {

                }
            }
        }

        private static void OnShowGUI(UnityModManager.ModEntry modEntry)
        {
            DoBuildSaveList();
        }

        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }

        private static void OnUpdate(UnityModManager.ModEntry arg1, float arg2)
        {
            if (!enabled)
                return;

            if (KeyDown(settings.QuickLoadKey) && saveFiles.Count > 0 && !isLoading)
            {
                string fileName = saveFiles[saveFiles.Count - 1].fileName;
                Dbgl($"Quick load {fileName}");
                isLoading = true;
                Singleton<TaskRunner>.Self.StartCoroutine(LoadGameFromArchive(fileName));
            }
        }

        [HarmonyPatch(typeof(GamingSolution), "Update")]
        static class GamingSolution_Update_Patch
        {
            static void Postfix()
            {
                if (!enabled)
                    return;

                if(settings.saveInterval > 0 && lastSave > 0 && Time.fixedTime > lastSave + settings.saveInterval * 60)
                {
                    Dbgl("performing auto save");
                    resetLastSave();
                    DoSaveFile(true);
                }

                if (KeyDown(settings.QuickSaveKey))
                {
                    DoSaveFile();
                }
            }

        }

        private static bool KeyDown(string key)
        {
            try {
                return Input.GetKeyDown(key);
            } 
            catch {
                return false;
            }
        }


        private static void DeleteSaveGame(string fileName)
        {
            string filePath = Path.Combine(GetSavesPath(), fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                if (File.Exists($"{filePath}.xml"))
                {
                    File.Delete($"{filePath}.xml");
                }
                DoBuildSaveList();
            }
        }
 

        private static void DoBuildSaveList()
        {
            saveFiles.Clear();
            string path = GetSavesPath();

            Dbgl($"Checking directory {path}");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                if (!Directory.Exists(path))
                {
                    Dbgl($"(OnShowGUI) Directory {path} does not exist and could not be created!");
                }
                return;
            }
            foreach (string file in Directory.GetFiles(path))
            {
                if (file.EndsWith(".xml"))
                    continue;
                CustomSaveFile csf = new CustomSaveFile(Path.GetFileName(file));
                if (csf.isValid())
                {
                    saveFiles.Add(csf);
                }
            }
            saveFiles.Sort(delegate (CustomSaveFile x, CustomSaveFile y)
            {
                string datex = x.fileName.Split('_')[2];
                string datey = y.fileName.Split('_')[2];
                return datex.CompareTo(datey);
            });

        }

        private static string GetSavesPath()
        {
            return "Mods\\SaveAnyTime\\saves";
        }



        private static Vector3 VectorFromString(string input)
        {
            if (input != null)
            {
                var vals = input.Split(',').Select(s => s.Trim()).ToArray();
                if (vals.Length == 3)
                {
                    Single v1, v2, v3;
                    if (Single.TryParse(vals[0], out v1) &&
                        Single.TryParse(vals[1], out v2) &&
                        Single.TryParse(vals[2], out v3))
                        return new Vector3(v1, v2, v3);
                    else
                        throw new ArgumentException();
                }
                else
                    throw new ArgumentException();
            }
            else
                throw new ArgumentException();
        }

    }
}
