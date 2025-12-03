using Harmony12;
using Pathea;
using Pathea.ActorNs;
using Pathea.BlackBoardNs;
using Pathea.DungeonModuleNs;
using Pathea.FavorSystemNs;
using Pathea.MessageSystem;
using Pathea.Missions;
using Pathea.ModuleNs;
using Pathea.Spawn;
using Pathea.SpawnNs;
using Pathea.TipsNs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;

namespace Invasion
{
    public partial class Main
    {
        private static readonly bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "Invasions " : "") + str);
        }
        public static Settings settings { get; private set; }
        public static bool enabled;
        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnToggle = OnToggle;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            MessageManager.Instance.Subscribe("WakeUpScreenEnd", new Action<object[]>(OnWakeUp));
            SceneManager.activeSceneChanged += ChangeScene;

            return;

            string bosslist = "";
            SqliteDataReader sqliteDataReader = LocalDb.cur.ReadFullTable("Monster");
            while (sqliteDataReader.Read())
            {
                SpawnInfoActor s = DbReader.ReadItem<SpawnInfoActor>(sqliteDataReader);
                if(s.isRare)
                    bosslist += + s.ID + "," + s.groupName+ ";";

            }
            Dbgl(bosslist);
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
            GUILayout.Label(string.Format("Chance per day of being invaded: <b>{00:F0}</b>", settings.ChanceInvade), new GUILayoutOption[0]);
            settings.ChanceInvade = (int)GUILayout.HorizontalSlider((float)Main.settings.ChanceInvade, 0f, 100f, new GUILayoutOption[0]);
            GUILayout.Space(10f);
            GUILayout.Label(string.Format("Chance of invaders bringing lower-tier gang: <b>{00:F0}</b>", settings.ChanceGang), new GUILayoutOption[0]);
            settings.ChanceGang = (int)GUILayout.HorizontalSlider((float)Main.settings.ChanceGang, 0f, 100f, new GUILayoutOption[0]);
            GUILayout.Space(10f);
            GUILayout.Label(string.Format("Minimum gang members: <b>{00:F0}</b>", settings.MinGang), new GUILayoutOption[0]);
            settings.MinGang = (int)GUILayout.HorizontalSlider((float)Main.settings.MinGang, 0f, 100f, new GUILayoutOption[0]);
            GUILayout.Space(10f);
            GUILayout.Label(string.Format("Maximum gang members: <b>{00:F0}</b>", settings.MaxGang), new GUILayoutOption[0]);
            settings.MaxGang = (int)GUILayout.HorizontalSlider((float)Main.settings.MaxGang, 0f, 100f, new GUILayoutOption[0]);
            GUILayout.Space(20f);

            settings.AllowRats = GUILayout.Toggle(settings.AllowRats, "Allow Rat King and Prince (potential respawn bug)", new GUILayoutOption[0]);
            GUILayout.Space(20f);

            settings.spawnRandomMonsters = GUILayout.Toggle(settings.spawnRandomMonsters, "Spawn random dungeon monsters throughout the map.", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            GUILayout.Label(string.Format("Minimum total monsters to spawn: <b>{00:F0}</b>", settings.spawnRandomMonstersMin), new GUILayoutOption[0]);
            settings.spawnRandomMonstersMin = (int)GUILayout.HorizontalSlider((float)Main.settings.spawnRandomMonstersMin, 1f, 2000f, new GUILayoutOption[0]);
            GUILayout.Space(10f);
            GUILayout.Label(string.Format("Maximum total monsters to spawn: <b>{00:F0}</b>", settings.spawnRandomMonstersMax), new GUILayoutOption[0]);
            settings.spawnRandomMonstersMax = (int)GUILayout.HorizontalSlider((float)Main.settings.spawnRandomMonstersMax, 1f, 2000f, new GUILayoutOption[0]);
            GUILayout.Space(10f);
            GUILayout.Label(string.Format("Maximum monsters per spawn point: <b>{00:F0}</b>", settings.spawnRandomMonstersGroupMax), new GUILayoutOption[0]);
            settings.spawnRandomMonstersGroupMax = (int)GUILayout.HorizontalSlider((float)Main.settings.spawnRandomMonstersGroupMax, 1f, 50f, new GUILayoutOption[0]);

            GUILayout.Space(20f);

            settings.RelationChange = GUILayout.Toggle(settings.RelationChange, "Invasion results affect universal favor", new GUILayoutOption[0]);
            GUILayout.Label(string.Format("Favor per boss tier killed: <b>{00:F0}</b>", settings.RelationPointsPerMonsterTier), new GUILayoutOption[0]);
            settings.RelationPointsPerMonsterTier = (int)GUILayout.HorizontalSlider((float)Main.settings.RelationPointsPerMonsterTier, 0f, 20f, new GUILayoutOption[0]);
        }

        private static int randSeed = (int)DateTime.UtcNow.Ticks + UnityEngine.Random.Range(0, 9999);
        private static System.Random rand = new System.Random(randSeed);

        private static void OnWakeUp(object[] obj)
        {
            if (!enabled)
                return;

            SceneManager.activeSceneChanged -= ChangeScene;
            SceneManager.activeSceneChanged += ChangeScene;

            if (agent != null)
            {
                Module<SpawnMgr>.Self.RemoveActorAgent(agent);
                agent.Destroy();
            }
            Module<AreaTriggerManager>.Self.DestroyTrigger(triggerID);
            monsterAlive = false;

            //TestSpawnAllMonsters();
            //SpawnAllPoints();
            //return;


            int chance = new IntR(0, 100).GetValue(rand);

            Dbgl("Chance: " + chance);

            if (chance >= settings.ChanceInvade)
                return;

            bossMonster = GetRandomMonster(rand, Get1337Level());

            gangSpawned = false;
            int chanceGang = new IntR(0, 100).GetValue(rand);
            if (Get1337Level() > 1 && chanceGang < settings.ChanceGang)
            {
                gangSpawned = true;
            }
            areaTrigger = Module<AreaTriggerManager>.Self.CreateAreaTrigger(triggerID, bossMonster.pos.Value, "Main", 40, true);
            monsterAlive = true;
            monsterName = bossMonster.name;
            char[] vowels = { 'A', 'E', 'I', 'O', 'U' };
            Singleton<TipsMgr>.Instance.SendSystemTip((vowels.Contains(monsterName[0]) ? "An " : "A ") + monsterName + " is attacking " + bossMonster.pos.Key + (gangSpawned?" and they brought a gang":"") + "!", SystemTipType.danger);

        }

        private static Vector3 GetGroupPos(Vector3 pos, int i, float distance, float variation)
        {
            float posx = pos.x;
            float posz = pos.z;

            float d1 = distance + distance * i / 4;
            float d2 = d1 / 2;
            switch (i % 8)
            {
                case 0:
                    posx += d1;
                    break;
                case 1:
                    posz += d1;
                    break;
                case 2:
                    posx -= d1;
                    break;
                case 3:
                    posz -= d1;
                    break;
                case 4:
                    posx += d2;
                    posz += d2;
                    break;
                case 5:
                    posx -= d2;
                    posz -= d2;
                    break;
                case 6:
                    posx += d2;
                    posz -= d2;
                    break;
                case 7:
                    posx -= d2;
                    posz += d2;
                    break;
            }

            return new Vector3(posx + (rand.Next(-100, 100) / 100f * variation), pos.y, posz + (rand.Next(-100, 100) / 100f * variation));
        }

        private static void ChangeScene(Scene arg0, Scene arg1)
        {
            if (!enabled)
                return;

            if (arg1.name == "Main")
            {
                SceneManager.activeSceneChanged -= ChangeScene;
                SpawnBossMonster();
                SpawnGangMonsters();
                SpawnRandomMonsters();
            }
        }

        private static void SpawnBossMonster()
        {
            if (monsterAlive)
            {
                Vector3 pos = GetValidPos(bossMonster.pos.Value);
                agent = new BossActorAgent(pos, bossMonster.rot, bossMonster.id, new Action<ActorAgent>(MonsterDeath));

                Module<SpawnMgr>.Self.AddActorAgent(agent);
                agent.Spawn();
            }
        }

        private static void SpawnGangMonsters()
        {
            if(gangSpawned) {
                Dbgl("spawning gang members");
                int gangNo = new IntR(settings.MinGang, settings.MaxGang).GetValue(rand);
                for (int i = 0; i < gangNo; i++)
                {
                    int l337 = new IntR(1, Get1337Level() - 1).GetValue(rand);
                    Monster mLow = GetRandomMonster(rand, l337);

                    Vector3 pos = GetValidPos(GetGroupPos(bossMonster.pos.Value, i, 8f, 4f));
                    if (pos == Vector3.zero)
                    {
                        Dbgl("gang member invalid position");
                        if (gangNo < settings.MaxGang * 2)
                        {
                            gangNo++;
                        }
                        continue;
                    }
                    Dbgl("gang member valid position");
                    ActorAgent agentLow = new ActorAgent(pos, bossMonster.rot, mLow.id, new Action<ActorAgent>(MonsterGangDeath));
                    Module<SpawnMgr>.Self.AddActorAgent(agentLow);
                    agentLow.Spawn();
                }
            }
        }

        private static void SpawnRandomMonsters()
        {
            if (!settings.spawnRandomMonsters)
                return;

            int x1 = -600;
            int x2 = 1000;
            int z1 = -900;
            int z2 = 900;
            int numMonsters = rand.Next(settings.spawnRandomMonstersMin, settings.spawnRandomMonstersMax + 1);
            int area = (x2 - x1) * (z2 - z1);
            float distance = Mathf.Sqrt(area / numMonsters / 4);
            int lvl = Math.Min(Module<Player>.Self.ActorLevel, 60);
            List<Vector3> points = new List<Vector3>();
            int c = 0;
            while (points.Count < numMonsters)
            {
                Vector3 v = new Vector3(rand.Next(x1, x2), 0, rand.Next(z1, z2));
                /*
                foreach (Vector3 p in points)
                {
                    if (Vector3.Distance(v, p) < distance)
                    {
                        continue;
                    }
                }
                */
                Vector3 valid = GetValidPos(v);
                if (valid != Vector3.zero)
                {
                    int idx = rand.Next((int)Math.Round(Math.Max(0, (lvl / 60f) * MonstersDungeon.Length - MonstersDungeon.Length / 2f)), (int)Math.Round(Math.Min(MonstersDungeon.Length - 1, (lvl / 60f) * MonstersDungeon.Length + MonstersDungeon.Length / 2f)));
                    Monster mm = MonstersDungeon[idx];

                    int gangNo = rand.Next(1, settings.spawnRandomMonstersGroupMax + 1);

                    for (int i = 0; i < gangNo; i++)
                    {
                        Vector3 pos = GetValidPos(GetGroupPos(valid, i, 2f, 1f));
                        if (pos != Vector3.zero)
                        {
                            BossActorAgent agentM = new BossActorAgent(pos, Vector3.forward, mm.id, new Action<ActorAgent>(MonsterGangDeath));
                            Module<SpawnMgr>.Self.AddActorAgent(agentM);
                            agentM.Spawn();
                            points.Add(pos);
                        }
                    }

                }
                c++;
                if (c > numMonsters * 100)
                {
                    break;
                }
            }
        }

        private static Vector3 GetValidPos(Vector3 pos)
        {
            Ray ray = default;
            RaycastHit[] hits = new RaycastHit[4];
            pos.y = 0;
            ray.origin = pos + Vector3.up * 1000f;
            ray.direction = Vector3.down;
            int num = Physics.RaycastNonAlloc(ray, hits, 2000f, 65792);
            if(num == 0)
            {
                return Vector3.zero;
            }
            for (int i = 0; i < num; i++)
            {
                RaycastHit raycastHit = hits[i];
                if (raycastHit.collider.CompareTag("Ground"))
                {
                    return raycastHit.point;
                }
            }
            return Vector3.zero;
        }

        private static void TestSpawnAllMonsters()
        {
            var allMonsters = new Monster[MonstersLow.Length + MonstersMid.Length + MonstersHigh.Length + MonstersElite.Length];
            MonstersLow.CopyTo(allMonsters, 0);
            MonstersMid.CopyTo(allMonsters, MonstersLow.Length);
            MonstersHigh.CopyTo(allMonsters, MonstersLow.Length + MonstersMid.Length);
            MonstersElite.CopyTo(allMonsters, MonstersLow.Length + MonstersMid.Length + MonstersHigh.Length);

            foreach (Monster mm in allMonsters)
            {
                ActorAgent aa = new ActorAgent(mm.pos.Value, mm.rot, mm.id, new Action<ActorAgent>(MonsterDeath));
                Dbgl("monster: " + mm.id);
                Module<SpawnMgr>.Self.AddActorAgent(aa);
                aa.Spawn();
            }
        }

        private static ActorAgent agent;
        private static AreaTrigger areaTrigger;
        private static readonly int triggerID = 999999999;
        private static bool monsterAlive = false;
        private static string monsterName = "monster";
        private static Monster bossMonster;
        private static bool gangSpawned = false;

        public static void MonsterDeath(ActorAgent agent)
        {
            monsterAlive = false;
            agent.Destroy();
            Module<SpawnMgr>.Self.RemoveActorAgent(agent);
            Module<AreaTriggerManager>.Self.DestroyTrigger(triggerID);
            if(settings.RelationChange)
            {
                Module<FavorManager>.Self.GiveGiftToAllFavorObject(Get1337Level() * settings.RelationPointsPerMonsterTier);
                Singleton<TipsMgr>.Instance.SendSimpleTip("You defeated the "+ monsterName+"! There is much rejoicing!");
            }
        }
        public static void MonsterGangDeath(ActorAgent agent)
        {
            agent.Destroy();
            Module<SpawnMgr>.Self.RemoveActorAgent(agent);
        }
        private static int Get1337Level()
        {
            int curMaxLevel = Module<TrialRandomDungeonManager>.Self.GetUnlimitedDungeonMaxLevel();
            int lvl = Module<Player>.Self.ActorLevel;
            if (lvl > 60 && Module<GlobleBlackBoard>.Self.HasInfo("afterfight") && curMaxLevel == 100)
            {
                return 5;
            }
            if (lvl > 45)
                return 4;
            if (lvl > 30)
                return 3;
            if (lvl > 15)
                return 2;
            return 1;
        }

        private static Monster GetRandomMonster(System.Random rand, int l337)
        {

            Monster[] Monsters = MonstersWeenie;
            switch (l337)
            {
                case 5:
                    Monsters = MonstersElite;
                    Dbgl("u r 1337");
                    break;
                case 4:
                    Monsters = MonstersHigh;
                    break;
                case 3:
                    Monsters = MonstersMid;
                    break;
                case 2:
                    Monsters = MonstersLow;
                    break;
            }

            int i = new IntR(1, Monsters.Length).GetValue(rand) - 1;

            while (!settings.AllowRats && Monsters[i].id > 20020000)
            {
                i = new IntR(1, Monsters.Length).GetValue(rand) - 1;
            }

            Monster m = Monsters[i];

            int highestPos = Positions.Length;

            // cut out places you can't access

            string amber = Module<GlobleBlackBoard>.Self.GetInfo("amber");
            string bridge = Module<GlobleBlackBoard>.Self.GetInfo("Portiabridge");
            string southblock = Module<GlobleBlackBoard>.Self.GetInfo("southblock");

            if (amber != "1")
                highestPos -= 3;
            else if (southblock != "2")
                highestPos -= 2;

            int p = new IntR(1, highestPos).GetValue(rand) - 1;

            m.pos = Positions[p];
            return m;
        }

        private class Monster {

            public Monster(int _id, string _name, int exp = 0)
            {
                id = _id;
                name = _name;
            }

            public int id;
            public KeyValuePair<string, Vector3> pos = new KeyValuePair<string, Vector3>( "Nowhere", Vector3.zero );
            public Vector3 rot = Vector3.forward;
            public string name;
        }

        private static readonly Monster[] MonstersDungeon = new Monster[]
        {
            new Monster(11005, "Bandirat"), // 5
            new Monster(10011005, "Bandirat"), // 5
            new Monster(11006, "Bandirat"), // 6
            new Monster(10011006, "Bandirat"), // 6
            new Monster(11007, "Bandirat"), // 7
            new Monster(10011007, "Bandirat"), // 7
            new Monster(11008, "Bandirat"), // 8
            new Monster(10011008, "Bandirat"), // 8
            new Monster(11009, "Bandirat"), // 9
            new Monster(10011009, "Bandirat"), // 9
            new Monster(11010, "Bandirat"), // 10
            new Monster(18010, "Slow Gooey"), // 10
            new Monster(10011010, "Bandirat"), // 10
            new Monster(11011, "Bandirat"), // 11
            new Monster(18011, "Slow Gooey"), // 11
            new Monster(10011011, "Bandirat"), // 11
            new Monster(11012, "Bandirat"), // 12
            new Monster(18012, "Slow Gooey"), // 12
            new Monster(22012, "Plierimp"), // 12
            new Monster(10011012, "Bandirat"), // 12
            new Monster(11013, "Bandirat"), // 13
            new Monster(18013, "Slow Gooey"), // 13
            new Monster(22013, "Plierimp"), // 13
            new Monster(10011013, "Bandirat"), // 13
            new Monster(11014, "Bandirat"), // 14
            new Monster(18014, "Slow Gooey"), // 14
            new Monster(19014, "Masked Fiend"), // 14
            new Monster(22014, "Plierimp"), // 14
            new Monster(27014, "Sentidog T-45"), // 14
            new Monster(10011014, "Bandirat"), // 14
            new Monster(11015, "Bandirat"), // 15
            new Monster(18015, "Slow Gooey"), // 15
            new Monster(19015, "Masked Fiend"), // 15
            new Monster(21015, "Jump Dancer"), // 15
            new Monster(22015, "Plierimp"), // 15
            new Monster(27015, "Sentidog T-45"), // 15
            new Monster(10011015, "Bandirat"), // 15
            new Monster(11016, "Bandirat"), // 16
            new Monster(18016, "Slow Gooey"), // 16
            new Monster(19016, "Masked Fiend"), // 16
            new Monster(21016, "Jump Dancer"), // 16
            new Monster(22016, "Plierimp"), // 16
            new Monster(27016, "Sentidog T-45"), // 16
            new Monster(10011016, "Bandirat"), // 16
            new Monster(11017, "Bandirat"), // 17
            new Monster(18017, "Slow Gooey"), // 17
            new Monster(19017, "Masked Fiend"), // 17
            new Monster(21017, "Jump Dancer"), // 17
            new Monster(22017, "Plierimp"), // 17
            new Monster(27017, "Sentidog T-45"), // 17
            new Monster(10011017, "Bandirat"), // 17
            new Monster(11018, "Bandirat"), // 18
            new Monster(18018, "Slow Gooey"), // 18
            new Monster(19018, "Masked Fiend"), // 18
            new Monster(21018, "Jump Dancer"), // 18
            new Monster(22018, "Plierimp"), // 18
            new Monster(27018, "Sentidog T-45"), // 18
            new Monster(10011018, "Bandirat"), // 18
            new Monster(30021018, "Jump Dancer"), // 18
            new Monster(11019, "Bandirat"), // 19
            new Monster(18019, "Slow Gooey"), // 19
            new Monster(19019, "Masked Fiend"), // 19
            new Monster(21019, "Jump Dancer"), // 19
            new Monster(22019, "Plierimp"), // 19
            new Monster(27019, "Sentidog T-45"), // 19
            new Monster(10011019, "Bandirat"), // 19
            new Monster(30022019, "Plierimp"), // 19
            new Monster(11020, "Bandirat"), // 20
            new Monster(18020, "Slow Gooey"), // 20
            new Monster(19020, "Masked Fiend"), // 20
            new Monster(21020, "Jump Dancer"), // 20
            new Monster(22020, "Plierimp"), // 20
            new Monster(27020, "Sentidog T-45"), // 20
            new Monster(10011020, "Bandirat"), // 20
            new Monster(10029000, "Backerat"), // 20
            new Monster(30019020, "Masked Fiend"), // 20
            new Monster(18021, "Slow Gooey"), // 21
            new Monster(19021, "Masked Fiend"), // 21
            new Monster(21021, "Jump Dancer"), // 21
            new Monster(22021, "Plierimp"), // 21
            new Monster(27021, "Sentidog T-45"), // 21
            new Monster(10011021, "Bandirat"), // 21
            new Monster(10029021, "Backerat"), // 21
            new Monster(10030021, "Redrat"), // 21
            new Monster(18022, "Slow Gooey"), // 22
            new Monster(19022, "Masked Fiend"), // 22
            new Monster(21022, "Jump Dancer"), // 22
            new Monster(22022, "Plierimp"), // 22
            new Monster(27022, "Sentidog T-45"), // 22
            new Monster(10011022, "Bandirat"), // 22
            new Monster(10029022, "Backerat"), // 22
            new Monster(10030022, "Redrat"), // 22
            new Monster(18023, "Slow Gooey"), // 23
            new Monster(19023, "Masked Fiend"), // 23
            new Monster(21023, "Jump Dancer"), // 23
            new Monster(22023, "Plierimp"), // 23
            new Monster(27023, "Sentidog T-45"), // 23
            new Monster(10011023, "Bandirat"), // 23
            new Monster(10029023, "Backerat"), // 23
            new Monster(10030023, "Redrat"), // 23
            new Monster(18024, "Slow Gooey"), // 24
            new Monster(19024, "Masked Fiend"), // 24
            new Monster(21024, "Jump Dancer"), // 24
            new Monster(22024, "Plierimp"), // 24
            new Monster(27024, "Sentidog T-45"), // 24
            new Monster(10011024, "Bandirat"), // 24
            new Monster(10029024, "Backerat"), // 24
            new Monster(10030024, "Redrat"), // 24
            new Monster(18025, "Slow Gooey"), // 25
            new Monster(19025, "Masked Fiend"), // 25
            new Monster(21025, "Jump Dancer"), // 25
            new Monster(22025, "Plierimp"), // 25
            new Monster(27025, "Sentidog T-45"), // 25
            new Monster(10029025, "Backerat"), // 25
            new Monster(10030025, "Redrat"), // 25
            new Monster(10030026, "Redrat"), // 25
            new Monster(10030027, "Redrat"), // 25
            new Monster(10030028, "Redrat"), // 25
            new Monster(10030029, "Redrat"), // 25
            new Monster(18026, "Slow Gooey"), // 26
            new Monster(19026, "Masked Fiend"), // 26
            new Monster(21026, "Jump Dancer"), // 26
            new Monster(22026, "Plierimp"), // 26
            new Monster(27026, "Sentidog T-45"), // 26
            new Monster(10029026, "Backerat"), // 26
            new Monster(18027, "Slow Gooey"), // 27
            new Monster(19027, "Masked Fiend"), // 27
            new Monster(21027, "Jump Dancer"), // 27
            new Monster(22027, "Plierimp"), // 27
            new Monster(27027, "Sentidog T-45"), // 27
            new Monster(10029027, "Backerat"), // 27
            new Monster(18028, "Slow Gooey"), // 28
            new Monster(19028, "Masked Fiend"), // 28
            new Monster(21028, "Jump Dancer"), // 28
            new Monster(22028, "Plierimp"), // 28
            new Monster(27028, "Sentidog T-45"), // 28
            new Monster(10029028, "Backerat"), // 28
            new Monster(18029, "Slow Gooey"), // 29
            new Monster(19029, "Masked Fiend"), // 29
            new Monster(21029, "Jump Dancer"), // 29
            new Monster(22029, "Plierimp"), // 29
            new Monster(27029, "Sentidog T-45"), // 29
            new Monster(10029029, "Backerat"), // 29
            new Monster(18030, "Slow Gooey"), // 30
            new Monster(19030, "Masked Fiend"), // 30
            new Monster(21030, "Jump Dancer"), // 30
            new Monster(22030, "Plierimp"), // 30
            new Monster(120030, "Tunnel Worm"), // 30
            new Monster(130030, "Cell Worm"), // 30
            new Monster(120031, "Tunnel Worm"), // 31
            new Monster(130031, "Cell Worm"), // 31
            new Monster(120032, "Tunnel Worm"), // 32
            new Monster(130032, "Cell Worm"), // 32
            new Monster(10011025, "Bandirat"), // 32
            new Monster(120033, "Tunnel Worm"), // 33
            new Monster(130033, "Cell Worm"), // 33
            new Monster(10030030, "Redrat"), // 33
            new Monster(120034, "Tunnel Worm"), // 35
            new Monster(160043, "Flame Variant"), // 35
            new Monster(160044, "Flame Variant"), // 35
            new Monster(160045, "Flame Variant"), // 35
            new Monster(10029030, "Backerat"), // 35
            new Monster(130034, "Cell Worm"), // 36
            new Monster(27030, "Sentidog T-45"), // 38
            new Monster(120035, "Tunnel Worm"), // 39
            new Monster(27031, "Sentidog T-45"), // 40
            new Monster(27034, "Sentidog T-45"), // 40
            new Monster(130035, "Cell Worm"), // 40
            new Monster(150040, "Lost Variant"), // 41
            new Monster(160040, "Flame Variant"), // 41
            new Monster(27032, "Sentidog T-52"), // 42
            new Monster(27033, "Sentidog T-52"), // 42
            new Monster(150041, "Lost Variant"), // 42
            new Monster(160041, "Flame Variant"), // 43
            new Monster(170040, "Miner Variant"), // 44
            new Monster(170044, "Miner Variant"), // 44
            new Monster(150044, "Lost Variant"), // 45
            new Monster(170041, "Miner Variant"), // 45
            new Monster(170045, "Miner Variant"), // 45
            new Monster(27046, "Sentidog T-52"), // 46
            new Monster(27047, "Sentidog T-52"), // 46
            new Monster(150042, "Lost Variant"), // 46
            new Monster(150045, "Lost Variant"), // 46
            new Monster(150043, "Lost Variant"), // 47
            new Monster(240047, "Patrolcraft T-21"), // 47
            new Monster(250047, "The Watchman"), // 47
            new Monster(260047, "Sentinel"), // 47
            new Monster(270047, "Shield Sentry"), // 47
            new Monster(160042, "Flame Variant"), // 48
            new Monster(250048, "The Watchman"), // 48
            new Monster(170042, "Miner Variant"), // 49
            new Monster(170043, "Miner Variant"), // 50
        };

        private static readonly Monster[] MonstersWeenie = new Monster[]
        {
            new Monster(23135, "Poppycock"), //16
            new Monster(2113, "Balloon Urchin"), //13
            new Monster(4120, "Vampanda"), //20
            new Monster(20003011, "Hermit Snaillob"), //11
            new Monster(20024010, "Bandirat Prince"), //10
        };

        private static readonly Monster[] MonstersLow = new Monster[]
        {
            new Monster(100016, "Chemical Dropout"), //16
            new Monster(140030, "Bikini Flippers"), //30
            new Monster(31125, "Courtious Bunny"), //25
            new Monster(12025, "Flurpee"), //25
            new Monster(15125, "Mrs. Ladybug"), //25
            new Monster(2125, "Balloon Urchin"), //25
            new Monster(140035, "Bikini Flippers"), //35
            new Monster(20003045, "Hermit Snaillob"), //45
            new Monster(6145, "Snake in a Box"), //45
            new Monster(20024010, "Bandirat Prince"), //10
        };

        private static readonly Monster[] MonstersMid = new Monster[]
        {
            new Monster(110030, "Rock On"), //30
            new Monster(100019, "Chemical Dropout"),  //19
            new Monster(180034, "Chief Honcho"), //34
            new Monster(20028025, "Rat King"),  //25
            new Monster(280047, "Bleeding Heart"), //48
            new Monster(20024050, "Bandirat Prince"), //50
        };
        private static readonly Monster[] MonstersHigh = new Monster[]
        {
            new Monster(200045, "Piggy-bot 002"), //40
            new Monster(39035, "Captain Crabbo"), //35
            new Monster(100022, "Chemical Dropout"), //22
            new Monster(180050, "Chief Honcho"), //50
            new Monster(110033, "Rock On"), //38
            new Monster(190045, "Piggy-bot 007"), //42
            new Monster(230047, "Piggy-bot 032"), //48
            new Monster(20028050, "Rat King"),  //50
        };
        private static readonly Monster[] MonstersElite = new Monster[]
        {
            new Monster(100025, "Chemical Dropout"), //25
            new Monster(180053, "Chief Honcho"), //55
            new Monster(110034, "Rock On"), //45
            new Monster(290050, "All-Source AI"), //99 
            new Monster(190055, "Piggy-bot 007"), //45
            new Monster(16250, "Pirate Queen"), //Everglade
            new Monster(220055, "Forgotten Knight"), //100
            new Monster(220050, "Rogue Knight"), //100
        };

        private static readonly KeyValuePair<string,Vector3>[] Positions = new KeyValuePair<string, Vector3>[]
        {
            new KeyValuePair<string, Vector3>("Center Plaza", new Vector3(208.9f, 46.6f, -112.1f)),
            new KeyValuePair<string, Vector3>("Sophie's field", new Vector3(-35.4f, 49.3f, -160.2f)),
            new KeyValuePair<string, Vector3>("Sophie's farm", new Vector3(-136.9f, 54.2f, -197.0f)),
            new KeyValuePair<string, Vector3>("the tree farm", new Vector3(-298.1f, 79.5f, 92.1f)),
            new KeyValuePair<string, Vector3>("Portia Harbour", new Vector3(-201.2f, 28.0f, -540.6f)),
            new KeyValuePair<string, Vector3>("Amber Island", new Vector3(239.7f, 31.5f, -325.9f)),
            new KeyValuePair<string, Vector3>("Ingall's Mine", new Vector3(812.7f, 0.9f, -743.2f)),
            new KeyValuePair<string, Vector3>("Southblock", new Vector3(336.3f, 33.9f, -428.5f)), 
        };
    }
}
