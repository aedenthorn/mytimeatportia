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
using Pathea.GameFlagNs;
using Pathea.HomeNs;
using Pathea.InputSolutionNs;
using Pathea.MessageSystem;
using Pathea.ModuleNs;
using Pathea.NpcRepositoryNs;
using Pathea.PlayerMissionNs;
using Pathea.RiderNs;
using Pathea.ScenarioNs;
using Pathea.ScreenMaskNs;
using Pathea.StageNs;
using Pathea.SummaryNs;
using Pathea.Times;
using Pathea.TipsNs;
using Pathea.UISystemNs;
using System;
using System.Collections;
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
                        if (GUILayout.Button(csf.GetSaveTitle(), new GUILayoutOption[]{
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


        private static void DoSaveFile()
        {
            if (Player.Self == null || Module<Player>.Self == null || Module<Player>.Self.actor == null)
                return;

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

            List<NPCMeta> npcs = new List<NPCMeta>();
            foreach(NpcData data in Module<NpcRepository>.Self.NpcInstanceDatas)
            {
                int instanceId = data.id;
                Actor actor = Module<ActorMgr>.Self.Get(instanceId);
                if (actor == null)
                    continue;
                string scene = actor.SceneName;
                string pos = actor.gamePos.ToString().Trim(new char[] { '(', ')' });
                NPCMeta npc = new NPCMeta();
                npc.id = instanceId;
                npc.scene = scene;
                npc.pos = pos;
                npcs.Add(npc);
            }
            List<RideableMeta> rideables = new List<RideableMeta>();
            foreach(int uid in Module<RidableModuleManager>.Self.GetAllRidableUid())
            {
                IRidable r = Module<RidableModuleManager>.Self.GetRidable(uid);

                if (r == null)
                    continue;
                string pos = r.GetPos().ToString().Trim(new char[] { '(', ')' });
                RideableMeta rideable = new RideableMeta();
                rideable.id = uid;
                rideable.pos = pos;
                rideables.Add(rideable);
            }
            SaveMeta save = new SaveMeta();
            save.NPClist = npcs;
            save.RideableList = rideables;

            System.Xml.Serialization.XmlSerializer writer =
                new System.Xml.Serialization.XmlSerializer(typeof(SaveMeta));

            var path2 = Path.Combine(GetSavesPath(),$"{fileName}.xml");
            System.IO.FileStream file = System.IO.File.Create(path2);
            writer.Serialize(file, save);
            file.Close();

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

        private static IEnumerator LoadGameFromArchive(string fileName)
        {
            string filePath = Path.Combine(GetSavesPath(), fileName);
            CustomSaveFile csf = new CustomSaveFile(fileName);
            if (!csf.isValid())
            {
                Dbgl("invalid save name:" + filePath);
                yield break;
            }
            if (!File.Exists(filePath))
            {
                Dbgl("file does not exist:" + filePath);
                yield break;
            }

            Module<GameConfigModule>.Self.IgnoreCloseDoorAudio = true;

            Singleton<GameRunner>.Instance.Rest();
            Singleton<LoadingMask.Mgr>.Instance.Begin(null);
            yield return null;

            Singleton<SleepTipMgr>.Self.SleepState(true);

            Dbgl("Checking if in game");
            int state = (int)typeof(GameMgr).GetField("state", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Module<GameMgr>.Self);
            Dbgl("state: " + state);
            if (state != (int)typeof(GameMgr).GetNestedType("State", BindingFlags.NonPublic | BindingFlags.Instance).GetField("Max", BindingFlags.Static | BindingFlags.Public).GetValue(Module<GameMgr>.Self))
            {
                Dbgl("Terminating");
                Singleton<ModuleMgr>.Instance.Terminate();
            }
            else
            {
                Dbgl("not in game");
            }
            UIStateMgr.Instance.PopToState(UIStateMgr.StateType.Play, true);


            Dbgl("Initializing modules");
            Singleton<ModuleMgr>.Instance.Init();

            Dbgl("Setting state");
            AccessTools.FieldRefAccess<GameMgr, object>(Module<GameMgr>.Self, "state") = typeof(GameMgr).GetNestedType("State", BindingFlags.NonPublic | BindingFlags.Instance).GetField("LoadGame", BindingFlags.Static | BindingFlags.Public).GetValue(Module<GameMgr>.Self);
            Dbgl("state: " + AccessTools.FieldRefAccess<GameMgr, object>(Module<GameMgr>.Self, "state"));

            lastLoadedSave = csf;
            Module<ScenarioModule>.Self.EndLoadEventor += OnSceneLoaded;

            Dbgl("Loading Archive");
            if (!Singleton<Archive>.Instance.LoadArchive(filePath))
            {
                Dbgl("Load archive failed:" + filePath);
                yield break;
            }

            Dbgl("Starting story");
            Module<Story>.Self.Start();

            yield break;
        }

        private static CustomSaveFile lastLoadedSave;

        private static void OnSceneLoaded(ScenarioModule.Arg arg)
        {
            Module<ScenarioModule>.Self.EndLoadEventor -= OnSceneLoaded;
            Module<Player>.Self.GamePos = VectorFromString(lastLoadedSave.position);

            Module<SleepModule>.Self.GetType().GetMethod("PlayerWakeUpAfterArchive", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Module<SleepModule>.Self, new object[] { });

            //Module<SleepModule>.Self.GetType().GetMethod("ShowSleepMask", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Module<SleepModule>.Self, new object[] { false });

            Module<SleepModule>.Self.WakeUpScreenMaskFinishedEvent?.Invoke();

            Singleton<SleepTipMgr>.Self.SleepState(false);

            GameDLCRewardsModule.Self.GetType().GetMethod("CheckAndOpenAllDlc", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Module<GameDLCRewardsModule>.Self, new object[] { });

            if (Module<EGMgr>.Self.IsEngagement())
            {
                Dbgl("Engagement is active");
                EGDate date = AccessTools.FieldRefAccess<EGMgr, EGDate>(Module<EGMgr>.Self, "mDate");
                GameDateTime dateBegin = AccessTools.FieldRefAccess<EGDate, GameDateTime>(date, "mBeginTimer");
                if(Module<TimeManager>.Self.DateTime > dateBegin)
                {
                    date.Start();
                }
                else if(Module<TimeManager>.Self.DateTime > dateBegin - EGConst.Spawn_Hour_1)
                {
                    Singleton<TipsMgr>.Instance.SendSystemTip(string.Format(TextMgr.GetStr(100507, -1), TextMgr.GetStr(AccessTools.FieldRefAccess<EGDate, int>(date, "mTipTypeID"), -1)), SystemTipType.warning);
                }
                else if (Module<TimeManager>.Self.DateTime > dateBegin - EGConst.Spawn_Hour_2)
                {
                    date.GetType().GetMethod("InitProjectMap", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(date, new object[] { });
                    Actor mActor = AccessTools.FieldRefAccess<EGDate, Actor>(date, "mActor");
                    AccessTools.FieldRefAccess<EGDate, Actor>(date, "mActor").SetBehaviorValue("EGActor", Module<Player>.Self.actor);
                    AccessTools.FieldRefAccess<EGDate, Actor>(date, "mActor").SetBehaviorValue("EGDate", EGData.GetDatePlace(AccessTools.FieldRefAccess<EGDate, int>(date, "mDateID")));
                    AccessTools.FieldRefAccess<EGDate, SharedInt>(date, "mForceValue") = mActor.GetBehaviorVariable<SharedInt>("EGForce");
                    AccessTools.FieldRefAccess<EGDate, SharedInt>(date, "mMoodValue") = mActor.GetBehaviorVariable<SharedInt>("EGMood");
                    AccessTools.FieldRefAccess<EGDate, SharedIntList>(date, "mEventCount") = mActor.GetBehaviorVariable<SharedIntList>("EGEventIDs");
                    AccessTools.FieldRefAccess<EGDate, List<EGRoot>>(date, "mRoots") = mActor.behavior.FindTasks<EGRoot>();
                    Singleton<TipsMgr>.Instance.SendSystemTip(string.Format(TextMgr.GetStr(100506, -1), TextMgr.GetStr(AccessTools.FieldRefAccess<EGDate, int>(date, "mTipTypeID"), -1)), SystemTipType.warning);
                }
            }


            
            string filePath = Path.Combine(GetSavesPath(), $"{lastLoadedSave.fileName}.xml");
            try
            {
                System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(SaveMeta));
                StreamReader file = new StreamReader($"{filePath}");
                SaveMeta save = (SaveMeta)reader.Deserialize(file);
                file.Close();

                foreach (NPCMeta npc in save.NPClist)
                {
                    Actor actor = Module<ActorMgr>.Self.Get(npc.id);
                    if (actor != null)
                    {
                        Module<ActorMgr>.Self.MoveToScenario(actor, npc.scene, VectorFromString(npc.pos));
                    }
                }
                if(save.RideableList != null)
                {
                    foreach (RideableMeta r in save.RideableList)
                    {
                        IRidable rideable = Module<RidableModuleManager>.Self.GetRidable(r.id);

                        if (rideable == null)
                            continue;
                        Dbgl("got rideable "+r.id);
                        Actor actor = rideable.GetActor();
                        if(actor != null)
                        {
                            Dbgl("got rideable actor " + actor.ActorName);
                            actor.gamePos = VectorFromString(r.pos);
                            actor.RefreshPos();
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Dbgl("Problem with meta file: "+ex);
            }
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
