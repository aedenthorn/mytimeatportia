using Harmony12;
using Pathea;
using Pathea.BlackBoardNs;
using Pathea.DungeonModuleNs;
using Pathea.FavorSystemNs;
using Pathea.MessageSystem;
using Pathea.Missions;
using Pathea.ModuleNs;
using Pathea.Spawn;
using Pathea.SpawnNs;
using Pathea.TipsNs;
using Redmine.Net.Api.Async;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityModManagerNet;

namespace Invasion
{
    public class Main
    {
        private static readonly bool isDebug = false;

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
            GUILayout.Label(string.Format("Chance of invaders bringing lower-tier gang: <b>{00:F0}</b>", settings.ChanceGang), new GUILayoutOption[0]);
            settings.ChanceGang = (int)GUILayout.HorizontalSlider((float)Main.settings.ChanceGang, 0f, 100f, new GUILayoutOption[0]);
            settings.RelationChange = GUILayout.Toggle(settings.RelationChange, "Invasion results affect universal favor", new GUILayoutOption[0]);
            //settings.AllowKnight = GUILayout.Toggle(settings.AllowKnight, "Allow Forgotten Knight to invade", new GUILayoutOption[0]);
            settings.AllowRats = GUILayout.Toggle(settings.AllowRats, "Allow Rat King and Prince (potential respawn bug)", new GUILayoutOption[0]);
        }

        private static int randSeed = (int)DateTime.UtcNow.Ticks + UnityEngine.Random.Range(0, 9999);
        private static System.Random rand = new System.Random(randSeed);

        private static void OnWakeUp(object[] obj)
        {
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

            Monster m = GetRandomMonster(rand, Get1337Level());

            agent = new ActorAgent(m.pos.Value, m.rot, m.id, new Action<ActorAgent>(MonsterDeath));

            Module<SpawnMgr>.Self.AddActorAgent(agent);
            agent.Spawn();

            gangSpawned = false;

            int chanceGang = new IntR(0, 100).GetValue(rand);
            if (Get1337Level() > 1 && chanceGang < settings.ChanceGang)
            {
                gangSpawned = true;
                int gangNo = new IntR(settings.MinGang, settings.MaxGang).GetValue(rand);
                for (int i = 0; i < gangNo; i++)
                {
                    int l337 = new IntR(1, Get1337Level()-1).GetValue(rand);
                    Monster mLow = GetRandomMonster(rand, l337);

                    float posx = m.pos.Value.x;
                    float posz = m.pos.Value.z;

                    float d1 = 8f + 8f * i / 4;
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

                    ActorAgent agentLow = new ActorAgent(new Vector3(posx, m.pos.Value.y,posz), m.rot, mLow.id, new Action<ActorAgent>(MonsterGangDeath));
                    Module<SpawnMgr>.Self.AddActorAgent(agentLow);
                    agentLow.Spawn();
                }
            }

            areaTrigger = Module<AreaTriggerManager>.Self.CreateAreaTrigger(triggerID, m.pos.Value, "Main", 40, true);
            monsterAlive = true;
            monsterName = m.name;
            char[] vowels = { 'A', 'E', 'I', 'O', 'U' };
            Singleton<TipsMgr>.Instance.SendSystemTip((vowels.Contains(monsterName[0]) ? "An " : "A ") + monsterName + " is attacking " + m.pos.Key + (gangSpawned?" and they brought a gang":"") + "!", SystemTipType.danger);
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
        private static bool gangSpawned = false;

        public static void MonsterDeath(ActorAgent agent)
        {
            monsterAlive = false;
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
