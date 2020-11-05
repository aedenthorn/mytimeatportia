using Harmony12;
using Pathea;
using Pathea.ActorNs;
using Pathea.DungeonModuleNs;
using Pathea.GameFlagNs;
using Pathea.ModuleNs;
using Pathea.ScenarioNs;
using Pathea.Spawn;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;
using static Harmony12.AccessTools;

namespace InstantKill
{
    public class Main
    {
        public static Settings settings { get; private set; }
        public static bool enabled;
        
        public static List<int> vacuumed;
        public static int pages = 3;
        private static readonly bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? typeof(Main).Namespace + " " : "") + str);
        }

        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnToggle = OnToggle;
            modEntry.OnUpdate = OnUpdate;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

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
            settings.KillEnable = GUILayout.Toggle(settings.KillEnable, "Enable Killing", new GUILayoutOption[0]);
            GUILayout.Space(10f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Kill Toggle Key", new GUILayoutOption[0]);
            settings.KillToggleButtom = GUILayout.TextField(settings.KillToggleButtom, new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            GUILayout.Space(10f);

            GUILayout.Label(string.Format("Kill Poll Interval: <b>{0}s</b>", settings.KillIntervalSec), new GUILayoutOption[0]);
            settings.KillIntervalSec = (int)GUILayout.HorizontalSlider((float)settings.KillIntervalSec, 1f, 10f, new GUILayoutOption[0]);
            GUILayout.Space(10f);

            GUILayout.Label(string.Format("Kill Max Distance: <b>{0}</b>", settings.KillDistance), new GUILayoutOption[0]);
            settings.KillDistance = (int)GUILayout.HorizontalSlider((float)settings.KillDistance, 1f, 100f, new GUILayoutOption[0]);
            GUILayout.Space(20f);


        }

        private static float secs;

        private static void OnUpdate(UnityModManager.ModEntry arg1, float arg2)
        {
            if (Input.GetKeyDown(settings.KillToggleButtom))
            {
                settings.KillEnable = !settings.KillEnable;
            }

            if (Time.time - secs > settings.KillIntervalSec)
            {
                secs = Time.time;
                KillMonsters();
            }
        }

        private static void KillMonsters()
        {
            if (!settings.KillEnable || !Singleton<GameFlag>.Instance.Gaming)
                return;
            try
            {
                Dbgl($"killing monsters in {ScenarioModule.Self.CurrentScenarioName}");

                if (ScenarioModule.Self.IsMainScene())
                {
                    List<ActorAgent> actorAgents = FieldRefAccess<SpawnMgr, List<ActorAgent>>("mActorAgent").Invoke(Module<SpawnMgr>.Self);

                    Dbgl($"actorAgents: {actorAgents.Count}");

                    foreach (ActorAgent aa in actorAgents)
                    {
                        Actor a = FieldRefAccess<ActorAgent, Actor>("_actor").Invoke(aa);
                        if (a == null)
                        {
                            continue;
                        }
                        if (Vector2.Distance(a.gamePos, Player.Self.GamePos) < settings.KillDistance)
                        {
                            a.Kill();
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
                        if (Vector2.Distance(a.gamePos, Player.Self.GamePos) < settings.KillDistance)
                        {
                            a.Kill();

                        }
                    }
                    List<Actor> actors = FieldRefAccess<DynamicRoomSceneManager, List<Actor>>("MonsterListForDelete").Invoke(Module<DynamicRoomSceneManager>.Self);
                    Dbgl($"dynamic room monsters: {actors.Count}");
                    foreach (Actor a in actors)
                    {
                        if (a == null)
                        {
                            continue;
                        }
                        if (Vector2.Distance(a.gamePos, Player.Self.GamePos) < settings.KillDistance)
                        {
                            a.Kill();

                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Dbgl($"Error killing monsters:\r\n\r\n {ex}");
            }
        }

    }
}
