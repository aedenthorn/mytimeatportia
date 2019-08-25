using System;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;
using Harmony12;
using Pathea.UISystemNs;
using Pathea.ItemSystem;
using Pathea;
using System.Collections.Generic;
using SimpleJSON;
using Pathea.FavorSystemNs;
using Pathea.ModuleNs;
using Pathea.MessageSystem;
using Pathea.PlayerMissionNs;
using Pathea.HomeNs;
using Pathea.SkillNs;
using Pathea.Missions;
using static Harmony12.AccessTools;
using Pathea.ActorNs;
using UnityEngine.SceneManagement;
using Pathea.ConfigNs;
using Pathea.GameResPointNs;
using Pathea.StageNs;
using System.Linq;
using PatheaScript;
using Ccc;
using PatheaScriptExt;
using Pathea.BlackBoardNs;

namespace Cheats
{
    public static partial class Main
    {
        private static bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                UnityEngine.Debug.Log((pref ? "Cheats " : "") + str);
        }
        public static bool enabled;

        public static Settings settings { get; private set; }

        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            MessageManager.Instance.Subscribe("WakeUpScreenEnd", new Action<object[]>(OnWakeUp));
            

            SceneManager.activeSceneChanged += ChangeScene;
            assetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/untitled");
            if (assetBundle != null)
            {
                customTextures["Nora"] = assetBundle.LoadAsset<Texture>("Nora.png");
                customTextures["Phyllis"] = assetBundle.LoadAsset<Texture>("Phyllis.png");
                customTextures["Penny"] = assetBundle.LoadAsset<Texture>("Penny.png");
                customTextures["Ginger"] = assetBundle.LoadAsset<Texture>("Ginger.png");
                customTextures["Sonia"] = assetBundle.LoadAsset<Texture>("Sonia.png");
                customTextures["Linda_Pants018"] = assetBundle.LoadAsset<Texture>("Linda_Pants018.png");
            }


        }

        private static void OnWakeUp(object[] obj)
        {
            //DumpStuff();
        }

        private static string GetModDir(string name = "")
        {
            string file = Application.dataPath;
            if (Application.platform == RuntimePlatform.OSXPlayer)
            {
                file += "/../../";
            }
            else if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                file += "/../";

            }

            string uri = "file:///" + file + "Mods/MarriageMod/" + name;
            return uri;
        }

        private static AssetBundle assetBundle = null;
        private static Dictionary<string, Texture> customTextures = new Dictionary<string, Texture>();
        private static int[] textureActors = new int[] { 4000035, 4000141, 4000006, 4000093, 4000033};

        private static void ChangeScene(Scene oldScene, Scene newScene)
        {
            Dbgl("new scene: " + newScene.name);

            FieldRef<Actor, SkinnedMeshRenderer> rendererByRef = FieldRefAccess<Actor, SkinnedMeshRenderer>("skinnedMeshRenderer");
            SkinnedMeshRenderer skinnedMeshRenderer = null;

            Dictionary<int, Actor> textureActors = new Dictionary<int, Actor>();


            for(int i = 0; i < Main.textureActors.Length; i++)
            {
                Actor a = Module<ActorMgr>.Self.Get(Main.textureActors[i]);
                if(a != null && a.InActiveScene && customTextures.ContainsKey(a.ActorName) && customTextures[a.ActorName] != null)
                {
                    skinnedMeshRenderer = rendererByRef(a);
                    if (skinnedMeshRenderer != null)
                        skinnedMeshRenderer.material.SetTexture("_MainTex", customTextures[a.ActorName]);
                }
            }
        }


        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (GUILayout.Button("New Game+", new GUILayoutOption[]{
                        GUILayout.Width(150f)
                    }))
            {
                NewGamePlus();
            }
        }

        private static void NewGamePlus()
        {
            MethodInfo dynMethod = Module<GlobleBlackBoard>.Self.GetType().GetMethod("OnInit", BindingFlags.NonPublic | BindingFlags.Instance);
            dynMethod.Invoke(Module<GlobleBlackBoard>.Self, new object[] { });

            FieldRef<Story, PsScriptMgr> ScriptMgr = FieldRefAccess<Story, PsScriptMgr>("mScriptMgr");
            ScriptMgr(Module<Story>.Self) = PsScriptMgr.Create(new CCFactory());

            /*
            FieldRef<FavorManager, Dictionary<int, FavorObject>> FavorDict = FieldRefAccess<FavorManager, Dictionary<int, FavorObject>>("mFavorDict");
            Dictionary<int, FavorObject> mFavorDict = FavorDict(Module<FavorManager>.Self);
            Dictionary<int, FavorObject> tFavorDict = new Dictionary<int, FavorObject>();
            foreach(KeyValuePair<int,FavorObject> p in mFavorDict)
            {
                int id = p.Key;
                FavorObject f = p.Value;
                tFavorDict.Add(id,new FavorObject(id, f.NpcType));
            }
            FavorDict(Module<FavorManager>.Self) = tFavorDict;
            */

            MethodInfo dynMethod2 = Module<FavorManager>.Self.GetType().GetMethod("OnInit", BindingFlags.NonPublic | BindingFlags.Instance);
            dynMethod2.Invoke(Module<FavorManager>.Self, new object[] { });

            MethodInfo dynMethod3 = MissionManager.GetInstance.GetType().GetMethod("OnInit", BindingFlags.NonPublic | BindingFlags.Instance);
            dynMethod3.Invoke(MissionManager.GetInstance, new object[] { });

            MethodInfo dynMethod4 = Module<SceneItemManager>.Self.GetType().GetMethod("OnInit", BindingFlags.NonPublic | BindingFlags.Instance);
            dynMethod4.Invoke(Module<SceneItemManager>.Self, new object[] { });

            Module<TimeManager>.Self.SetDateTime(new Hont.GameDateTime(1, 1, 1, 3, 0, 0), true, TimeManager.JumpingType.System);


        }

        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }
        /*
        [HarmonyPatch(typeof(PackageUIBase), "ShowSplitWindow")]
        static class PackageUIBase_BagSplit
        {
            static void Prefix(ref Action<int> Confirm, ItemObject item, PackageUIBase __instance, bool ___IsSplitMode)
            {
                if (item != null && item.Number > 1)
                {
                    Confirm = delegate (int num)
                    {
                        ItemObject newItem = ItemObject.CreateItem(item.ItemDataId, num);
                        if (Module<Player>.Self.bag.AddItemFromSplit(newItem, false))
                        {
                            item.ChangeNumber(-num);
                            __instance.FreshCurpageItem();
                            __instance.playerItemBar.FreshItem();
                        }
                    };
                }
            }
        }
        */
        [HarmonyPatch(typeof(PackageUIBase), "BagClick", new Type[] { typeof(int) })]
        static class PackageUIBase_BagSelect
        {
            static bool Prefix(PackageUIBase __instance, int obj, bool ___IsSplitMode, int ___bagIndex)
            {
                if (obj == -1)
                    return true;

                ItemObject item = Module<Player>.Self.bag.GetItem(___bagIndex, obj);
                if (item != null && ___IsSplitMode && item.Number == 1)
                {
                    Module<Player>.Self.bag.AddItem(item, true);
                    __instance.FreshCurpageItem();
                    __instance.playerItemBar.FreshItem();
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Pathea.SkillNs.Factory), "CreateCmd", new Type[] { typeof(JSONNode) })]
        static class Factory_CreateCmd_Patch
        {
            static void Prefix(ref JSONNode node)
            {
                if (node["name"] == "ClashCmd" && Module<Player>.Self.actor.IsAnimTag("Chainsaw") || Module<Player>.Self.actor.IsAnimTag("Chainsaw_Cut") || Module<Player>.Self.actor.IsAnimTag("Chainsaw_Stand"))
                {
                    float asFloat = node["dmg"].AsFloat;
                    asFloat *= 4;
                    node["dmg"] = asFloat.ToString();
                }

            }
        }

        [HarmonyPatch(typeof(Player), "Jump", new Type[] { })]
        static class Player_Jump_Patch
        {
            static void Postfix()
            {
                //Module<Player>.Self.AddExp(400000, true);
                //Module<Player>.Self.bag.ChangeMoney(1000000, true, 0);
                //Object here is necessary.


                //Dbgl("OtherConfig: " + string.Format(str));

                //Debug.Log("new Vector3" + Module<Player>.Self.actor.gamePos.ToString() + ",");
                //Module<Player>.Self.bag.AddItem(ItemObject.CreateItem(1001402,1), true);



                //trialDungeonMonsterConfInfo.ActorID;
                //trialDungeonMonsterConfInfo.Faction;

                //ActorAgent aa new ActorAgent(vp, vr,);
                /*
                int id = ruleData.PickOneBossMonster();
                TrialDungeonMonsterConfInfo trialDungeonMonsterConfInfo = Module<DungeonConfCreator>.Self.RandomDungeonMonsterConfList.Find((TrialDungeonMonsterConfInfo m) => m.ID == id);
                if (trialDungeonMonsterConfInfo == null)
                {
                    Debug.LogError(string.Format("monster {0} not existed!", id));
                    return;
                }
                GameObject gameObject = spawnPoint.Spawn(new object[]
                {
                    trialDungeonMonsterConfInfo.ActorID,
                    trialDungeonMonsterConfInfo.Faction
                });
                if (gameObject != null)
                {
                    Actor component = gameObject.GetComponent<Actor>();
                    this.mCreatedActorList.Add(component);
                    Module<TrialRandomDungeonManager>.Self.AddMonster(component, trialDungeonMonsterConfInfo);
                    component.killEvent += Module<TrialRandomDungeonManager>.Self.MonsterBeKilledCBMethod;
                }
                */
                //FavorObject ff = FavorManager.Self.GetFavorObject(-1);
                //bool test = ff.IsDebut;

                if (false)
                {
                    FavorObject[] fa = FavorManager.Self.GetAllFavorObjects();

                    foreach (FavorObject f in fa)
                    {
                        if (!f.IsDebut || f.RelationshipType == FavorRelationshipType.Couple)
                        {
                            FavorManager.Self.GainFavorValue(f.ID, -1, 100);
                            if (false)
                            {
                                FavorRelationshipId relationId = FavorRelationshipId.UltimateCouple;
                                relationId = FavorRelationshipId.Friend;
                                FavorUtility.SetNpcRelation(f.ID, relationId, 1);
                                relationId = FavorRelationshipId.SoulMate;
                                FavorUtility.SetNpcRelation(f.ID, relationId, 1);
                            }
                        }
                    }
                    if (true)
                    {
                        Module<Player>.Self.bag.AddItem(ItemObject.CreateItem(7000041, 100), true);
                        Module<Player>.Self.bag.AddItem(ItemObject.CreateItem(8000019, 100), true);
                        Module<Player>.Self.bag.ChangeMoney(100000, true, 0);
                    }

                }
            }
        }
        [HarmonyPatch(typeof(Player), "Roll", new Type[] { })]
        static class Player_Roll_Patch
        {
            static void Postfix()
            {
                //Module<FavorManager>.Self.GainFavorValue(4000141, -1, 1000);
                if (false)
                    TimeManager.Self.JumpTimeByGameTime(60 * 60);



                if (false)
                {
                    Module<Player>.Self.bag.AddItem(ItemObject.CreateItem(7000041, 100), true);
                    Module<Player>.Self.bag.AddItem(ItemObject.CreateItem(8000019, 100), true);
                }

                //relationId = FavorRelationshipId.SoulMate;
                //FavorUtility.SetNpcRelation(f.ID, relationId, 1);

            }
        }


        [HarmonyPatch(typeof(TextMgr), "Get", new Type[] { typeof(int) })]
        static class TextMgr_Get_Patch
        {
            static void Postfix(ref String __result)
            {
                if (__result.Contains("I've have"))
                {
                    __result = "[1|4000001], I've done a bunch of work, check it out!";

                }
            }
        }

        //[HarmonyPatch(typeof(FarmUpgradeMgr), "GetUpgradeItemCost", new Type[] { typeof(FarmBuildingEnum), typeof(int) })]
        static class FarmUpgradeMgr_GetUpgradeItemCost_Patch
        {
            static void Postfix(List<IdCount> __result)
            {
                if (__result == null)
                    return;
                foreach (IdCount idc in __result)
                {
                    try
                    {
                        Module<Player>.Self.bag.AddItem(ItemObject.CreateItem(idc.id, idc.count), true);
                    }
                    catch
                    {

                    }
                }
            }
        }
        //[HarmonyPatch(typeof(FarmUpgradeMgr), "GetUpgradeMoneyCost", new Type[] { typeof(FarmBuildingEnum), typeof(int)})]
        static class FarmUpgradeMgr_GetUpgradeMoneyCost_Patch
        {
            static void Postfix(int __result)
            {
                Module<Player>.Self.bag.ChangeMoney(__result, true, 0);
            }
        }

        private static void DumpStuff()
        {

            /*
            SqliteDataReader sqliteDataReader = LocalDb.cur.ReadFullTable("Fish_Food");

            List<String> list = new List<String>();
            while (sqliteDataReader.Read())
            {
                List<int> ints = new List<int>();
                int id = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("ID"));
                ints.Add(id);
                ints.Add(sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("Type")));
                ints.Add(sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("Quantity")));
                ints.Add(sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("PriceBase")));
                ints.Add(sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("ExtraRewards")));
                string name = Module<ItemDataMgr>.Self.GetItemName(id);
                list.Add(name+","+string.Join(",", ints.Select(x => x.ToString()).ToArray()));

            }
            Dbgl(string.Format(string.Join("\r\n",list.ToArray())));
            */

            MethodInfo dynMethod = (new FishFeedItem()).GetType().GetMethod("LoadData", BindingFlags.Static | BindingFlags.NonPublic);
            List<FishFeedItem> list = (List<FishFeedItem>)dynMethod.Invoke(new FishFeedItem(), new object[] { });

            //FieldRef<FishFeedItem, List<FishFeedItem>> datas = FieldRefAccess<FishFeedItem, List<FishFeedItem>>("datas");
            //List<FishFeedItem> list = datas(new FishFeedItem());

            string str = DumpList(list);

            Dbgl(string.Format(str));

            //FieldRef<FishData, List<FishData>> datas2 = FieldRefAccess<FishData, List<FishData>>("datas");
            //List<FishData> list2 = datas2(new FishData());

            MethodInfo dynMethod2 = (new FishData()).GetType().GetMethod("LoadData", BindingFlags.Static | BindingFlags.NonPublic);
            List<FishData> list2 = (List<FishData>)dynMethod2.Invoke(new FishData(), new object[] { });

            string str2 = DumpList(list2);

            Dbgl(string.Format(str2));
            
        }

        private static string DumpList<T>(List<T> obj)
        {
            bool first = true;
            String str = "";
            foreach (object i in obj)
            {
                FieldInfo[] fields = i.GetType().GetFields(BindingFlags.Public |
                                              BindingFlags.NonPublic |
                                              BindingFlags.Instance);
                if (first)
                {
                    string[] head = new string[fields.Length];
                    for (int j = 0; j < fields.Length; j++)
                    {
                        head[j] = fields[j].Name;
                    }
                    str += string.Join(",", head) + "\r\n";
                }
                first = false;

                string name = "";

                if (fields[0].GetValue(i) != null)
                {
                    name = Module<ItemDataMgr>.Self.GetItemName((int)fields[0].GetValue(i));
                }

                string[] row = new string[fields.Length];
                for (int j = 0; j < fields.Length; j++)
                {
                    if (fields[j].GetValue(i) == null)
                        row[j] = "";
                    else row[j] = fields[j].GetValue(i).ToString();
                }
                str += name+","+ string.Join(",", row) + "\r\n";

            }
            return str;
        }

    }
}