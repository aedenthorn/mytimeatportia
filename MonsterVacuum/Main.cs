using Harmony12;
using Pathea;
using Pathea.ActorNs;
using Pathea.DungeonModuleNs;
using Pathea.FeatureNs;
using Pathea.GameFlagNs;
using Pathea.ItemDropNs;
using Pathea.MessageSystem;
using Pathea.ModuleNs;
using Pathea.ScenarioNs;
using Pathea.Spawn;
using Pathea.UISystemNs;
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

        private static readonly bool isDebug = false;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? typeof(Main).Namespace + " " : "") + str);
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
            modEntry.OnUpdate = OnUpdate;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private static void OnUpdate(UnityModManager.ModEntry arg1, float arg2)
        {
            if (Input.GetKeyDown(settings.VacuumButton))
            {
                VacuumLoot();
            }
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
            GUILayout.BeginHorizontal();
            GUILayout.Label("Vacuum Key", new GUILayoutOption[0]);
            settings.VacuumButton = GUILayout.TextField(settings.VacuumButton, new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            GUILayout.Space(10f);

            GUILayout.Label(string.Format("Vacuum Range: <b>{0}</b>", settings.VacuumRadius), new GUILayoutOption[0]);
            settings.VacuumRadius = (int)GUILayout.HorizontalSlider(settings.VacuumRadius, 1f, 1000f, new GUILayoutOption[0]);
            GUILayout.Space(20f);
        }

        private static void VacuumLoot()
        {
            if (Module<Player>.Self?.actor == null || !Singleton<GameFlag>.Instance.Gaming || UIStateMgr.Instance.BlockInput)
                return;
            try
            {
                Dbgl($"Vacuuming monsters in {ScenarioModule.Self.CurrentScenarioName}");

                Actor actor = null;

                if (ScenarioModule.Self.IsMainScene())
                {
                    List<ActorAgent> actorAgents = AccessTools.FieldRefAccess<SpawnMgr, List<ActorAgent>>("mActorAgent").Invoke(Module<SpawnMgr>.Self);

                    Dbgl($"actorAgents: {actorAgents.Count}");

                    foreach (ActorAgent aa in actorAgents)
                    {
                        Actor a = AccessTools.FieldRefAccess<ActorAgent, Actor>("_actor").Invoke(aa);
                        if (a == null)
                        {
                            continue;
                        }
                        Dbgl($"checking actor {a.ActorName}");
                        if (Vector3.Distance(a.gamePos, Player.Self.GamePos) < settings.VacuumRadius && !vacuumed.Contains(a.InstanceId))
                        {
                            if (actor == null || (Vector3.Distance(a.gamePos, Player.Self.GamePos) < Vector3.Distance(actor.gamePos, Player.Self.GamePos)))
                                actor = a;
                        }
                    }
                }
                else
                {

                    List<MonsterData> monsterList = TrialRandomDungeonManager.Self.monsterList;
                    Dbgl($"trial monsters: {monsterList.Count}");

                    foreach (MonsterData md in monsterList)
                    {
                        Actor a = md.actor;
                        if (a == null)
                        {
                            continue;
                        }
                        Dbgl($"checking actor {a.ActorName}");
                        if (Vector3.Distance(a.gamePos, Player.Self.GamePos) < settings.VacuumRadius && !vacuumed.Contains(a.InstanceId))
                        {
                            if (actor == null || (Vector3.Distance(a.gamePos, Player.Self.GamePos) < Vector3.Distance(actor.gamePos, Player.Self.GamePos)))
                            {
                                actor = a;
                            }
                        }
                    }
                    List<Actor> actors = AccessTools.FieldRefAccess<DynamicRoomSceneManager, List<Actor>>("MonsterListForDelete").Invoke(Module<DynamicRoomSceneManager>.Self);
                    Dbgl($"dynamic room monsters: {actors.Count}");
                    foreach (Actor a in actors)
                    {
                        if (a == null)
                        {
                            continue;
                        }
                        Dbgl($"checking actor {a.ActorName}");
                        if (Vector3.Distance(a.gamePos, Player.Self.GamePos) < settings.VacuumRadius && !vacuumed.Contains(a.InstanceId))
                        {
                            if (actor == null || (Vector3.Distance(a.gamePos, Player.Self.GamePos) < Vector3.Distance(actor.gamePos, Player.Self.GamePos)))
                                actor = a;
                        }
                    }
                }

                if (actor == null)
                {
                    Dbgl($"No monster in range");
                    return;
                }
                Dbgl($"Vacuuming monster: distance: {Vector3.Distance(actor.gamePos, Player.Self.GamePos)} drop group: {Module<ActorMgr>.Self.GetActorInfo(actor.TmpltId).dropGroup} monster: {actor.MonsterTag}");

                float scaleNum = 1f + Module<FeatureModule>.Self.ModifyFloat(FeatureType.MonsterDrop, new object[] { 1f });
                float expScale = actor.ExpScale;
                Module<ItemDropManager>.Self.DropItemListByIdWithScale(Module<ActorMgr>.Self.GetActorInfo(actor.TmpltId).dropGroup, actor.transform.position, scaleNum, expScale);
                vacuumed.Add(actor.InstanceId);
            }
            catch (Exception ex)
            {
                Dbgl($"Error dropping loot:\r\n\r\n {ex}");
            }
        }
    }
}
