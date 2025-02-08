using Harmony12;
using Pathea;
using Pathea.ConfigNs;
using Pathea.DungeonModuleNs;
using Pathea.ItemBoxNs;
using Pathea.MessageSystem;
using Pathea.ModuleNs;
using PatheaScriptExt;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;

namespace Chests
{
    public static class Main
    {
        public static bool enabled;
        private static bool isDebug = true;
        private static System.Random rand;
        private static List<BoxInfo> basicRewardList;
        private static List<DungeonChestConfInfo> medRewardList;
        private static List<Reward> eliteRewardList;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? typeof(Main).Namespace + " " : "") + str);
        }
        public static Settings settings { get; private set; }

        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);
            int randSeed = (int)DateTime.UtcNow.Ticks + UnityEngine.Random.Range(0, 9999);
            rand = new System.Random(randSeed);

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            GetRewardsList();
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {

            GUILayout.Label(string.Format("Number of Ordinary Chests: <b>{0:F0}</b>", settings.MaxNumberOfOrdinaryChests), new GUILayoutOption[0]);
            settings.MaxNumberOfOrdinaryChests = (int)GUILayout.HorizontalSlider((float)settings.MaxNumberOfOrdinaryChests, 1f, 1000f, new GUILayoutOption[0]);
            GUILayout.Space(10f);

            GUILayout.Label(string.Format("Number of Special Chests: <b>{0:F0}</b>", settings.MaxNumberOfAdvancedChests), new GUILayoutOption[0]);
            settings.MaxNumberOfAdvancedChests = (int)GUILayout.HorizontalSlider((float)settings.MaxNumberOfAdvancedChests, 1f, 1000f, new GUILayoutOption[0]);
            GUILayout.Space(10f);

            GUILayout.Label(string.Format("Number of Elite Chests: <b>{0:F0}</b>", settings.MaxNumberOfEliteChests), new GUILayoutOption[0]);
            settings.MaxNumberOfEliteChests = (int)GUILayout.HorizontalSlider((float)settings.MaxNumberOfEliteChests, 1f, 1000f, new GUILayoutOption[0]);
            GUILayout.Space(10f);

            GUILayout.Label(string.Format("Ordinary Chest Chance: <b>{0:F0}%</b>", settings.OrdinaryChestChance*100), new GUILayoutOption[0]);
            settings.OrdinaryChestChance = GUILayout.HorizontalSlider((int)(settings.OrdinaryChestChance * 10), 0f, 10f, new GUILayoutOption[0]) / 10f;
            GUILayout.Space(10f);

            GUILayout.Label(string.Format("Advanced Chest Chance: <b>{0:F0}%</b>", settings.AdvancedChestChance * 100), new GUILayoutOption[0]);
            settings.AdvancedChestChance = GUILayout.HorizontalSlider((int)(settings.AdvancedChestChance * 10), 0f, 10f, new GUILayoutOption[0]) / 10f;
            GUILayout.Space(10f);

            GUILayout.Label(string.Format("Elite Chest Chance: <b>{0:F0}%</b>", settings.EliteChestChance * 100), new GUILayoutOption[0]);
            settings.EliteChestChance = GUILayout.HorizontalSlider((int)(settings.EliteChestChance * 10), 0f, 10f, new GUILayoutOption[0]) / 10f;
            GUILayout.Space(20f);

        }

        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value; 
            return true; // Permit or not.
        }

        [HarmonyPatch(typeof(DynamicRoom), nameof(DynamicRoom.GenerateContent))]
        static class DynamicRoom_GenerateContent_Patch
        {
            static void Prefix(DynamicRoom __instance)
            {
                if (!enabled)
                    return;
            }
        }
        private static void BeginDropChests()
        {
            if(SceneManager.GetActiveScene().name != "Main")
            {
                Dbgl("Not in main scene!");
                return;
            }

            Dbgl("Removing chests!");
            int removed = 0;
            ItemBox[] boxes = TransRoot.self.GetComponentsInChildren<ItemBox>();
            for(int i = 0; i < boxes.Length; i++)
            {
                if (boxes[i].name.EndsWith("_mod_chest"))
                {
                    UnityEngine.Object.Destroy(boxes[i].gameObject);
                    removed++;
                }
            }
            Dbgl($"Removed {removed} chests!");

            Dbgl("Dropping chests!");

            int x1 = -600;
            int x2 = 1000;
            int z1 = -900;
            int z2 = 900;

            List<Vector3> points = new List<Vector3>();
            int c = 0;
            int percent = 0;
            while (points.Count < settings.MaxNumberOfOrdinaryChests + settings.MaxNumberOfAdvancedChests + settings.MaxNumberOfEliteChests)
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

                int level = points.Count < settings.MaxNumberOfOrdinaryChests ? 0 : (points.Count < settings.MaxNumberOfOrdinaryChests + settings.MaxNumberOfAdvancedChests ? 1 : 2);

                Vector3 valid = GetValidPos(v);
                if (valid != Vector3.zero)
                {
                    points.Add(valid);
                    DropChest(valid, level);
                }
                c++;
                if (c > (settings.MaxNumberOfOrdinaryChests + settings.MaxNumberOfEliteChests) * 10)
                {
                    break;
                }
            }
        }


        private static void DropChest(Vector3 valid, int level)
        {
            int r = rand.Next(eliteRewardList.Count);
            int r2 = rand.Next(3);
            float scale;
            string prefabPath;
            int rewardId = GetRewardId(r, r2, out scale, out prefabPath, level);

            bool playActorAnim = settings.PlayAnimation;

            GameObject gameObject = Singleton<ResMgr>.Instance.LoadSyncByType<GameObject>(AssetType.Mission, prefabPath);
            if (gameObject == null)
            {
                //Dbgl("game object is null!");
                return;
            }
            GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject);

            Transform mItemBoxRoot = new GameObject("ItemBox").transform;
            mItemBoxRoot.parent = TransRoot.self.transform;
            mItemBoxRoot.localPosition = Vector3.zero;

            gameObject2.transform.parent = mItemBoxRoot;
            ItemBox component = gameObject2.GetComponent<ItemBox>();
            if (component == null)
            {
                //Dbgl("item box is null!");
                return;
            }
            //Dbgl("Spawning chest!");
            component.name += "_mod_chest";
            component.path = prefabPath;
            if(level == 2)
                component.SetItems(rewardId, playActorAnim, scale);
            else
                component.SetItems(rewardId, playActorAnim);
            component.gamePos = valid;
        }

        private static Vector3 GetValidPos(Vector3 pos)
        {
            Ray ray = default;
            RaycastHit[] hits = new RaycastHit[4];
            pos.y = 0;
            ray.origin = pos + Vector3.up * 1000f;
            ray.direction = Vector3.down;
            int num = Physics.RaycastNonAlloc(ray, hits, 999f, 256);
            if (num == 0)
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


        private static void GetRewardsList()
        {
            basicRewardList = DbReader.Read<BoxInfo>(LocalDb.cur.ReadFullTable("The_Chest"), 20);

            medRewardList = DbReader.Read<DungeonChestConfInfo>(LocalDb.cur.ReadFullTable("Dungeon_Chest"), 20);

            eliteRewardList = new List<Reward>();
            
            SqliteDataReader sqliteDataReader = LocalDb.cur.ReadFullTable("Unlimited_DungeonN");
            while (sqliteDataReader.Read())
            {
                IntR range = new IntR(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("levelRange")), '~');
                string rawString = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("reward"));
                string[] rawRewards = rawString.Split(new char[]
                {
                    '|'
                });
                if (rawRewards.Length < 4)
                {
                    Debug.LogError("rewards length error! " + rawString);
                }
                else
                {
                    DoubleValue<int, FloatR>[] rewardsArray = new DoubleValue<int, FloatR>[4];
                    for (int j = 0; j < 4; j++)
                    {
                        string[] array5 = rawRewards[j].Split(new char[]
                        {
                            '_'
                        });
                        rewardsArray[j] = new DoubleValue<int, FloatR>(int.Parse(array5[0]), new FloatR(array5[1], '~'));
                    }
                    eliteRewardList.Add(new Reward(rewardsArray, range));
                }
            }
        }

        public static int GetRewardId(int eliteLevel, int rank, out float scale, out string path, int level)
        {
            path = "ItemBox/objects_box_baoxiang01_Anim";
            scale = 1f;
            int r;
            switch (level)
            {
                case 0:
                    r = rand.Next(basicRewardList.Count);
                    path = "ItemBox/objects_box_baoxiang01_Anim";
                    return basicRewardList[r].dropGroup;
                case 1:
                    r = rand.Next(medRewardList.Count);
                    path = medRewardList[r].Model_Path;
                    return medRewardList[r].Drop_Group;
                case 2:
                    if (rank >= eliteRewardList[eliteLevel].rewardsArray.Length)
                    {
                        return 0;
                    }
                    path = "ItemBox/objects_box_baoxiang02_Anim";
                    scale = eliteRewardList[eliteLevel].rewardsArray[rank].value1.GetLinerValue(eliteRewardList[eliteLevel].levelRange.GetLinearInterpolationValue(eliteLevel));
                    return eliteRewardList[eliteLevel].rewardsArray[rank].value0;
                default:
                    return 0;
            }
        }

        private class BoxInfo
        {
            [DbReader.DbFieldAttribute("ID", ',')]
            public int id;

            [DbReader.DbFieldAttribute("Scene_Name", ',')]
            public string sceneName;

            [DbReader.DbFieldAttribute("Coordinate", ',')]
            public Vector3 pos;

            [DbReader.DbFieldAttribute("Rotation", ',')]
            public float rot;

            [DbReader.DbFieldAttribute("Drop_Group", ',')]
            public int dropGroup;

            [DbReader.DbFieldAttribute("Model_Path", ',')]
            public string path;

            [DbReader.DbFieldAttribute("Play_Anim", ',')]
            public int playAnim;
        }
    }
}