using Ccc;
using Harmony12;
using Hont;
using Pathea;
using Pathea.ArchiveNs;
using Pathea.GameFlagNs;
using Pathea.InputSolutionNs;
using Pathea.ModuleNs;
using Pathea.ScenarioNs;
using Pathea.ScreenMaskNs;
using Pathea.SummaryNs;
using Pathea.Times;
using Pathea.UISystemNs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityModManagerNet;

namespace SaveAnyTime
{
    public class Main
    {
        private static bool isDebug = false;

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
            return true;
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (Player.Self != null && Module<Player>.Self != null  && Module<Player>.Self.actor != null )
            {
                if (GUILayout.Button("Save Now", new GUILayoutOption[]{
                        GUILayout.Width(150f)
                    }))
                {
                    DoSaveFile();
                }
                if (saveFiles.Count > 0)
                {
                    GUILayout.Space(20f);
                }
            }
            if (saveFiles.Count > 0)
            {
                GUILayout.Label("Load Game", new GUILayoutOption[0]);
                foreach (CustomSaveFile csf in saveFiles)
                {
                    if (GUILayout.Button(csf.GetSaveTitle(), new GUILayoutOption[]{
                    }))
                    {
                        LoadGameFromArchive(csf.fileName);
                        UnityModManager.UI.Instance.ToggleWindow();
                    }
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

        private static void DoSaveFile()
        {

            string path = GetSavesPath();

            Dbgl($"Checking directory {path}");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (!Directory.Exists(path))
            {
                Dbgl($"Directory {path} does not exist and could not be created!");
                return;
            }

            Dbgl("Building save file");

            SummaryPlayerIdentity curPlayerIdentity = Module<SummaryModule>.Self.GetCurPlayerIdentity();
            DateTime now = DateTime.Now;
            TimeSpan totalPlayTime = Module<SummaryModule>.Self.GetTotalPlayTime();
            GameDateTime dateTime = Module<TimeManager>.Self.DateTime;
            Dbgl("Building base name");
            string fileName = (string)Singleton<Archive>.Instance.GetType().GetMethod("GenSaveFileName", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Singleton<Archive>.Instance, new object[] { curPlayerIdentity, now, dateTime, totalPlayTime });
            Dbgl(fileName);
            string name = Module<Player>.Self.ActorName;
            Dbgl(name);
            string timeOfDay = $"{dateTime.Hour}h{dateTime.Minute}m";
            Dbgl(timeOfDay);
            string sceneName = Module<ScenarioModule>.Self.CurrentScenarioName;
            Dbgl(sceneName);
            string position = Module<Player>.Self.GamePos.ToString("F4").Trim(new char[] {'(',')'});
            fileName = $"{name}_{fileName}_{timeOfDay}_{sceneName}_{position}";
            Dbgl(fileName);
            string filePath = Path.Combine(path, fileName);
            Dbgl(filePath);
            Singleton<Archive>.Instance.SaveArchive(filePath);

            DoBuildSaveList();
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

        private static bool LoadGameFromArchive(string fileName)
        {
            string filePath = Path.Combine(GetSavesPath(), fileName);
            CustomSaveFile csf = new CustomSaveFile(fileName);
            if (!csf.isValid())
            {
                Dbgl("invalid save name:" + filePath);
                return false;
            }
            if (!File.Exists(filePath))
            {
                Dbgl("file does not exist:" + filePath);
                return false;
            }
            Singleton<GameRunner>.Instance.Rest();
            Singleton<LoadingMask.Mgr>.Instance.Begin(null);

            Singleton<SleepTipMgr>.Self.SleepState(true);

            Dbgl("Checking if in game");
            try
            {
                int state = (int)(AccessTools.FieldRefAccess<GameMgr, object>(Module<GameMgr>.Self, "state"));
                if (state != (int)typeof(GameMgr).GetNestedType("State", BindingFlags.NonPublic | BindingFlags.Instance).GetField("Max", BindingFlags.Static | BindingFlags.Public).GetValue(Module<GameMgr>.Self))
                {
                    Dbgl("state: " + state);
                    Dbgl("Terminating");
                    Singleton<ModuleMgr>.Instance.Terminate();
                }
            }
            catch
            {
                Dbgl("not in game");
                UIStateMgr.Instance.PopState(false);
            }

            Dbgl("Initializing modules");
            Singleton<ModuleMgr>.Instance.Init();

            Dbgl("Setting state");
            AccessTools.FieldRefAccess<GameMgr, object>(Module<GameMgr>.Self, "state") = typeof(GameMgr).GetNestedType("State", BindingFlags.NonPublic | BindingFlags.Instance).GetField("LoadGame", BindingFlags.Static | BindingFlags.Public).GetValue(Module<GameMgr>.Self);

            if (!Singleton<Archive>.Instance.LoadArchive(filePath))
            {
                Dbgl("Load archive failed:" + filePath);
                return false;
            }

            Module<Story>.Self.Start();
            lastLoadedPos = VectorFromString(csf.position);
            Module<ScenarioModule>.Self.TransferToScenario(csf.sceneName);
            Module<ScenarioModule>.Self.EndLoadEventor += OnSceneLoaded;
            int year = int.Parse(csf.date.Split('Y')[0]);
            int month = int.Parse(csf.date.Split('Y')[1].Split('M')[0]);
            int day = int.Parse(csf.date.Split('Y')[1].Split('M')[1].Split('D')[0]);
            Module<TimeManager>.Self.SetDateTime(new GameDateTime(year, month, day, csf.hour, csf.minute, 0), false, TimeManager.JumpingType.System);

            return true;
        }

        public static Vector3 lastLoadedPos;

        private static void OnSceneLoaded(ScenarioModule.Arg arg)
        {
            Module<ScenarioModule>.Self.EndLoadEventor -= OnSceneLoaded;
            Module<Player>.Self.GamePos = lastLoadedPos;
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
