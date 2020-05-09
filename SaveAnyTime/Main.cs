using BehaviorDesigner.Runtime;
using Ccc;
using Harmony12;
using Hont;
using Pathea;
using Pathea.ActorNs;
using Pathea.ArchiveNs;
using Pathea.Behavior;
using Pathea.DLCRewards;
using Pathea.EG;
using Pathea.FavorSystemNs;
using Pathea.HomeNs;
using Pathea.HomeViewerNs;
using Pathea.InputSolutionNs;
using Pathea.ModuleNs;
using Pathea.NpcRepositoryNs;
using Pathea.PlayerMissionNs;
using Pathea.RiderAdapterNs;
using Pathea.RiderNs;
using Pathea.ScenarioNs;
using Pathea.ScreenMaskNs;
using Pathea.StoreNs;
using Pathea.SummaryNs;
using Pathea.Times;
using Pathea.TipsNs;
using Pathea.UISystemNs;
using Pathea.WeatherNs;
using System;
using System.Collections;
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

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "SaveAnyTime " : "") + str);
        }
        public static bool enabled;
        public static Settings settings { get; private set; }

        // Send a response to the mod manager about the launch status, success or not.
        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnShowGUI = OnShowGUI;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            DoBuildSaveList();
            return true;
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label("Quick Save Key:", new GUILayoutOption[0]);
            settings.QuickSaveKey = GUILayout.TextField(settings.QuickSaveKey, new GUILayoutOption[0]);
            GUILayout.Label("Quick Load Key:", new GUILayoutOption[0]);
            settings.QuickLoadKey = GUILayout.TextField(settings.QuickLoadKey, new GUILayoutOption[0]);

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
                        if (GUILayout.Button(csf.saveTitle, new GUILayoutOption[]{
                    }))
                        {
                            Singleton<TaskRunner>.Self.StartCoroutine(LoadGameFromArchive(csf.fileName));
                            UnityModManager.UI.Instance.ToggleWindow();
                        }
                        if (GUILayout.Button("X", new GUILayoutOption[]{
                        GUILayout.Width(50f)
                    }))
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


        [HarmonyPatch(typeof(UIBaseSolution), "Update")]
        static class UIBaseSolution_Update_Patch
        {
            static void Postfix()
            {
                if (!enabled)
                    return;

                if (Input.GetKeyDown(settings.QuickLoadKey) && saveFiles.Count > 0)
                {
                    List<string> files = new List<string>();
                    foreach (CustomSaveFile csf in saveFiles)
                    {
                        files.Add(csf.fileName);
                    }
                    files.Sort(delegate (string x, string y)
                    {
                        string datex = x.Split('_')[2];
                        string datey = y.Split('_')[2];
                        return datex.CompareTo(datey);
                    });
                    string fileName = files[files.Count - 1];
                    Dbgl($"Quick load {fileName}");
                    Singleton<TaskRunner>.Self.StartCoroutine(LoadGameFromArchive(fileName));
                }
            }
        }
        [HarmonyPatch(typeof(GamingSolution), "Update")]
        static class GamingSolution_Update_Patch
        {
            static void Postfix()
            {
                if (!enabled)
                    return;

                if (Input.GetKeyDown(settings.QuickSaveKey))
                {
                    DoSaveFile();
                }
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

        [HarmonyPatch(typeof(PlayerMissionMgr), "FreshMissionState")]
        static class ScenarioModule_PostLoad_Patch
        {
            static void Prefix(PlayerMissionMgr __instance)
            {
                Dbgl("PlayerMissionMgr FreshState" + __instance.PublishedMission.Count);
            }
        }
    }
}
