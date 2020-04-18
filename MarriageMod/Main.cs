using Harmony12;
using UnityModManagerNet;
using System.Reflection;
using Pathea.FavorSystemNs;
using Pathea.NpcRepositoryNs;
using UnityEngine;
using System.Collections.Generic;
using Pathea.MG;
using Pathea.ModuleNs;
using Pathea;
using System;
using Pathea.ItemSystem;
using Pathea.BlackBoardNs;
using Pathea.HomeNs;
using System.Threading;
using Pathea.MessageSystem;
using Pathea.ActorNs;
using Pathea.ACT;
using RootMotion.FinalIK;
using Pathea.ScenarioNs;
using UnityEngine.SceneManagement;
using Pathea.EG;
using UnityEngine.Networking;
using System.Collections;

namespace MarriageMod
{
    public static partial class Main
    {
        public static bool enabled;
        public static Settings settings { get; private set; }
        private static AudioClip kissAudioClip = null;
        private static int kissAudioId = 514;
        private static List<Vector3> kissLocations = new List<Vector3>();

        // Send a response to the mod manager about the launch status, success or not.
        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            PreloadAudio pa = new PreloadAudio();
            pa.Start("Npc_Kiss.ogg");

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            harmony.Patch(AccessTools.Method(typeof(MGMgr), nameof(MGMgr.CanMarriage)), new HarmonyMethod(typeof(Main).GetMethod("MGMgr_CanMarriage_Patch_Prefix")));
            harmony.Patch(AccessTools.Method(typeof(FavorUtility), nameof(FavorUtility.GetGiftGainInfo)),new HarmonyMethod(typeof(Main).GetMethod("FavorUtility_GetGiftGainInfo_Patch_Prefix")));
            harmony.Patch(AccessTools.Method(typeof(Player), nameof(Player.CanExpressRuntime)),new HarmonyMethod(typeof(Main).GetMethod("Pathea_Player_CanExpressRuntime_Patch_Prefix")));

            SceneManager.activeSceneChanged += ChangeScene;

            return true;
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


        public class StaticCoroutine
        {
            private static StaticCoroutineRunner runner;

            public static Coroutine Start(IEnumerator coroutine)
            {
                EnsureRunner();
                return runner.StartCoroutine(coroutine);
            }

            private static void EnsureRunner()
            {
                if (runner == null)
                {
                    runner = new GameObject("[Static Coroutine Runner]").AddComponent<StaticCoroutineRunner>();
                    UnityEngine.Object.DontDestroyOnLoad(runner.gameObject);
                }
            }

            private class StaticCoroutineRunner : MonoBehaviour { }
        }

        private class PreloadAudio : MonoBehaviour
        {
            public void Start(string filename)
            {
                StaticCoroutine.Start(Coroutine(filename));
            }
            public static IEnumerator Coroutine(string filename)
            {
                string uri = GetModDir(filename);

                using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.OGGVORBIS))
                {
                    www.SendWebRequest();
                    yield return www;
                    if (www != null)
                    {
                        kissAudioClip = DownloadHandlerAudioClip.GetContent(www);
                    }

                }
            }
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            int spouse = GetCurrentSpouse();
            GUILayout.Label("Spouse set to <b>" + (spouse == 0 ? "None" : Module<NpcRepository>.Self.GetNpcName(spouse)) + "</b>:", new GUILayoutOption[0]);
            if (GUILayout.Button("None", new GUILayoutOption[]{
                GUILayout.Width(150f)
            }))
            {
                settings.CurrentSpouse = 0;
            }
            FavorObject[] fList = FavorManager.Self.GetAllShowFavorObjects();
            if (fList != null && fList.Length > 0)
            {
                foreach (FavorObject f in fList)
                {
                    if (f.RelationshipType == FavorRelationshipType.Couple)
                    {
                        if (GUILayout.Button(Module<NpcRepository>.Self.GetNpcName(f.ID), new GUILayoutOption[]{
                        GUILayout.Width(150f)
                    }))
                        {
                            settings.CurrentSpouse = f.ID;
                        }
                    }

                }
            }
            GUILayout.Space(10f);
            GUILayout.Label(string.Format("Number of wedding rings to sell per month: <b>{0}</b>", settings.RingsPerMonth), new GUILayoutOption[0]);
            settings.RingsPerMonth = (int)GUILayout.HorizontalSlider((float)settings.RingsPerMonth, 1f, 30f, new GUILayoutOption[0]);
            GUILayout.Space(10f);
            settings.SpousesKiss = GUILayout.Toggle(settings.SpousesKiss, "Allow spouses to kiss each other", new GUILayoutOption[0]);
            settings.KissSound = GUILayout.Toggle(settings.KissSound, "Use more realistic kissing sound for kisses", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            GUILayout.Label(string.Format("Minimum seconds between spouse kissing: <b>{0:F0}</b>", settings.MinKissingInterval), new GUILayoutOption[0]);
            settings.MinKissingInterval = (int)GUILayout.HorizontalSlider((float)Main.settings.MinKissingInterval, 2f, 30f, new GUILayoutOption[0]);
            GUILayout.Space(10f);
            GUILayout.Label(string.Format("Max distance for spouses to kiss automatically: <b>{0:F0}</b>", settings.MaxKissingDistance), new GUILayoutOption[0]);
            settings.MaxKissingDistance = GUILayout.HorizontalSlider(settings.MaxKissingDistance, 1f, 20f, new GUILayoutOption[0]);
        }


        public static bool MGMgr_CanMarriage_Patch_Prefix(int npcId, ref int result, ref bool __result)
        {
            if (!Main.enabled)
                return true;

            result = -1;
            if (GlobleBlackBoard.Self.HasInfo("Nomarry_" + npcId))
            {
                result = 11;
                __result = false;
                return false;
            }
            if (!FavorRelationshipUtil.CanPropose(npcId, out result))
            {
                __result = false;
                return false;
            }
            if (HomeModule.Self.HomeLevel < 1)
            {
                result = 8;
                __result = false;
                return false;
            }
            __result = true;
            return false;
        }

        public static bool FavorUtility_GetGiftGainInfo_Patch_Prefix(NpcData npcData, FavorManagerConfInfo favorConf, ItemBaseConfData itemBaseConf, ref int gainValue, ref FeeLevelEnum feeLevel, ref string replyText, ref GiftType gType)
        {
            if (!Main.enabled)
                return true;

            if (itemBaseConf.NameID == 271422) //heart knot
            {
                gainValue = 0;
                replyText = string.Empty;
                gType = GiftType.Normal;
                feeLevel = FeeLevelEnum.Refuse;
                int failIndex = -1;
                int id = npcData.id;

                Dbgl("Giving heart knot to :" + npcData.Name + " " + npcData.id + " " + npcData.factionId);

                if (Module<Player>.Self.CanExpress(id, out failIndex) && FavorRelationshipUtil.CheckExpress(id, ref failIndex))
                {
                    gainValue = FavorUtility.GetGiftGainValue(favorConf.FavorValues_Confession, itemBaseConf);
                    feeLevel = FeeLevelEnum.Confession;
                    gType = GiftType.Relation;
                }
                else
                {
                    gainValue = 0;
                    feeLevel = FeeLevelEnum.Refuse;
                    gType = GiftType.Relation;
                }

                replyText = FavorUtility.GetGiveGiftDialog(npcData, favorConf, itemBaseConf, feeLevel, failIndex);
                return false;
            }
            else if (itemBaseConf.NameID == 270956) // wedding ring
            {
                gainValue = 0;
                replyText = string.Empty;
                gType = GiftType.Normal;
                feeLevel = FeeLevelEnum.Refuse;
                int failIndex = -1;
                int id = npcData.id;

                if (Module<MGMgr>.Self.CanMarriage(id, out failIndex) && FavorRelationshipUtil.CheckPropose(id, ref failIndex))
                {
                    gainValue = FavorUtility.GetGiftGainValue(favorConf.FavorValues_Propose, itemBaseConf);
                    feeLevel = FeeLevelEnum.Propose;
                    gType = GiftType.Relation;
                }
                else
                {
                    gainValue = 0;
                    feeLevel = FeeLevelEnum.Refuse;
                    gType = GiftType.Relation;
                }

                replyText = FavorUtility.GetGiveGiftDialog(npcData, favorConf, itemBaseConf, feeLevel, failIndex);
                return false;
            }

            return true;
        }
        // allowing romancing while married

        public static bool Pathea_Player_CanExpressRuntime_Patch_Prefix(int npcId, ref int result, ref bool __result)
        {
            if (!Main.enabled)
                return true;
            result = -1;
            if (!Module<MGMgr>.Self.IsSingle())
            {
                FavorObject f = FavorManager.Self.GetFavorObject(npcId);
                if (f.RelationshipType == FavorRelationshipType.Couple)
                {
                    result = 3;
                    __result = false;
                }
                else
                {
                    result = -1;
                    __result = true;
                }
                return false;
            }
            return true;
        }

        public static void ResMgr_LoadSyncByType_Generic_Patch()
        {
            return;
        }

        public static bool ResMgr_LoadSyncByType_AudioClip_Patch_Prefix(ref AudioClip __result, AssetType assetType, string path, bool tryLoad, bool cache)
        {
            Dbgl("checking asset: " + path);
            if (!enabled || assetType != AssetType.Effect)
                return true;

            Dbgl("checking effect: " + path);

            if (path != null && path.Contains("Npc_Kiss"))
            {

            }
            return true;
        }

        public static void SetCurrentSpouse(int id)
        {
            if (id > 0)
            {
                Actor a = Module<ActorMgr>.Self.Get(id);
                if (a != null && !Module<MGMgr>.Self.IsPropose())
                {
                    Module <MGMgr>.Self.GetType().GetField("mActor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(Module<MGMgr>.Self, a);
                    Module <MGMgr>.Self.GetType().GetField("mNpcID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(Module<MGMgr>.Self, id);
                }

            }
            settings.CurrentSpouse = id;
        }

        public static int GetCurrentSpouse()
        {
            int currentSpouse = settings.CurrentSpouse;
            try
            {
                if (Module<MGMgr>.Self.IsPropose())
                    return Module<MGMgr>.Self.GetMarriageID();

                if (currentSpouse > 0 && (FavorManager.Self.GetFavorObject(currentSpouse) != null && FavorManager.Self.GetFavorObject(currentSpouse).RelationshipType != FavorRelationshipType.Couple))
                {
                    currentSpouse = 0;
                    List<FavorObject> fList = new List<FavorObject>(FavorManager.Self.GetAllShowFavorObjects());
                    foreach (FavorObject f in fList)
                    {
                        if (f.RelationshipType == FavorRelationshipType.Couple)
                            currentSpouse = f.ID;
                    }
                }
            }
            catch
            {
            }

            return currentSpouse;
        }

        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }

        private static List<FavorObject> GetSpouses()
        {
            List<FavorObject> spouseList = new List<FavorObject>();
            List<FavorObject> fList = new List<FavorObject>(FavorManager.Self.GetAllShowFavorObjects());
            foreach (FavorObject fo in fList)
            {
                if (fo.RelationshipType == FavorRelationshipType.Couple)
                {
                    spouseList.Add(fo);
                }
            }
            return spouseList;
        }

        private static bool IsSpouse(int id)
        {

            FavorObject f = Module<FavorManager>.Self.GetFavorObject(id);
            return (f != null && f.RelationshipType == FavorRelationshipType.Couple);
        }
        private static bool IsSpouse(NpcData obj)
        {
            FavorObject f = FavorManager.Self.GetFavorObject(obj.id);
            return (f.RelationshipType & FavorRelationshipType.Couple) == FavorRelationshipType.Couple;
        }

        private static Timer timer = null;
        private static bool FirstRun = true;

        private static void ChangeScene(Scene oldScene, Scene newScene)
        {
            Dbgl("new scene: " + newScene.name);
            List<FavorObject> spouseList = GetSpouses();
            foreach(FavorObject f in spouseList)
            {
                ResetOneSpouseAnimation(Module<ActorMgr>.Self.Get(f.ID));
            }

            if (newScene.name != "Game" && FirstRun)
            {
                FirstRun = false;
                //Module<SleepModule>.Self.WakeUpScreenMaskFinishedEvent += AfterWakeUp;
                timer = new Timer(TimerCallback, "KissingTimer", TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(1));
                rand = new System.Random(randSeed);
            }

        }

        private static void AfterWakeUp() { 

            EndAnimation.Invoke();
            timer = new Timer(TimerCallback, "KissingTimer", TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1));
            rand = new System.Random(randSeed);
        }

        private static int randSeed = (int)DateTime.UtcNow.Ticks + UnityEngine.Random.Range(0, 9999);
        private static System.Random rand;
        private static double lastKiss = 0;
        private static TurnOffGravity turnOffGravity = new TurnOffGravity();
        private static Dictionary<int,double> kissingSpouses = new Dictionary<int, double>();
        private static string animName = "Interact_Kiss";
        private static string iKName = "IKPeck";

        private static void TimerCallback(object state)
        {
            double currentTime = Module<TimeManager>.Self.TotalSecond / Module<TimeManager>.Self.TimeScale;

            foreach (KeyValuePair<int,double> k in kissingSpouses)
            {
                if(k.Value + 6 < currentTime)
                {
                    Actor a = ActorMgr.Self.Get(k.Key);
                    if (a != null)
                        ResetOneSpouseAnimation(a);
                }

            }

            if (!enabled || !settings.SpousesKiss || Module<ScenarioModule>.Self.CurrentScenarioName == "Main" || Module<ScenarioModule>.Self.CurrentScenarioName == "food")
                return;

            double nextKissTime = lastKiss + settings.MinKissingInterval;

            if (currentTime < nextKissTime)
                return;

            List<FavorObject> spouseList = GetSpouses();
            List<Actor> aSpouseList = new List<Actor>();
            aSpouseList.Add(Module<Player>.Self.actor);

            foreach (FavorObject f in spouseList)
            {
                Actor a = ActorMgr.Self.Get(f.ID);
                if (a == null)
                    continue;

                if (a.InActiveScene)
                {
                    aSpouseList.Add(a);
                }

            }

            int n = aSpouseList.Count;
            while (n > 1)
            {
                n--;
                int k = rand.Next(n + 1);
                Actor value = aSpouseList[k];
                aSpouseList[k] = aSpouseList[n];
                aSpouseList[n] = value;
            }

            foreach (Actor a in aSpouseList)
            {
                if (currentTime < nextKissTime)
                    break;

                if (!a.CanInteract || a.InBattle || !a.Visible || a.IsActionRunning(ACType.Sleep) || a.IsActionRunning(ACType.Sit) || a.IsActionRunning(ACType.Conversation) || a.IsActionRunning(ACType.Interact) || kissingSpouses.ContainsKey(a.InstanceId) || Module<EGMgr>.Self.IsEngagementEvent(a.InstanceId) || a.OnBus || a.IsInteractive())
                {
                    continue; 
                }
                Vector3 pos1 = a.gamePos;
                foreach (Actor ao in aSpouseList)
                {
                    if (ao.InstanceId == a.InstanceId || !ao.CanInteract || ao.InBattle || !ao.Visible || ao.IsActionRunning(ACType.Sleep) || ao.IsActionRunning(ACType.Sit) || ao.IsActionRunning(ACType.Conversation) || ao.IsActionRunning(ACType.Interact) || kissingSpouses.ContainsKey(ao.InstanceId) || Module<EGMgr>.Self.IsEngagementEvent(ao.InstanceId) || ao.OnBus || ao.IsInteractive())
                    {
                        continue;
                    }
                    Vector3 pos2 = ao.gamePos;
                    float dist = Vector3.Distance(pos1, pos2);

                    if (dist < settings.MaxKissingDistance)
                    {

                        int maxR = Math.Max(50 - Math.Max(((int)Math.Round(currentTime - nextKissTime) - settings.MinKissingInterval),0), 2);
                        int chance = new IntR(1, maxR).GetValue(rand);

                        if (chance != 1)
                            continue;

                        float _radiusSelf = a.GetActorRadius();
                        float _radiusTar = ao.GetActorRadius();
                        float num = (Module<NpcRepository>.Self.GetNpcGender(a.InstanceId) != Gender.Male) ? .17f : .19f;
                        if(a.InstanceId == Module<Player>.Self.actor.InstanceId || ao.InstanceId == Module<Player>.Self.actor.InstanceId)
                        {
                            num = (Module<Player>.Self.GetGender() != Gender.Male) ? -0.05f : -0.03f;
                        }
                        float _dis = _radiusSelf + _radiusTar + num;

                        Vector3 _tarDir = ao.gamePos - a.gamePos;
                        _tarDir.y = 0f;
                        Vector3 tarPos;
                        if (Module<ScenarioModule>.Self.CurrentScenarioName == "Main")
                        {
                            Util.TryGetGroundPosition(out tarPos, a.gamePos, _tarDir, _dis, 3f, 3f, true, 0.1f, _radiusTar, 36);
                        }
                        else
                        {
                            Util.TryGetGroundPosition(out tarPos, a.gamePos, _tarDir, _dis, 2f, 2f, true, 0.1f, _radiusTar, 36);
                        }

                        float y = Mathf.Max(a.gamePos.y, tarPos.y);

                        Vector3 apos = a.gamePos;
                        Vector3 arot = a.gameRot.eulerAngles;

                        Vector3 _sefPos = a.gamePos;
                        _sefPos.y = y;
                        Vector3 _tarPos = tarPos;
                        _tarPos.y = y;
                        Vector3 _sefRot = Quaternion.LookRotation(_tarPos - _sefPos, Vector3.up).eulerAngles;
                        Vector3 _tarRot = Quaternion.LookRotation(_sefPos - _tarPos, Vector3.up).eulerAngles;
                        Vector3 vector = (_sefPos + _tarPos) * 0.5f;
                        if (a.TryDoAction(ACType.Transfer, ACTransferPara.Construct(string.Empty, _sefPos, _sefRot)))
                        {
                            if (ao.TryDoAction(ACType.Transfer, ACTransferPara.Construct(string.Empty, _tarPos, _tarRot)))
                            {
                                bool isTall = animName == "Interact_Kiss" && (Module<ActorMgr>.Self.GetActorInfo(a.TmpltId).modelType == "Npc_Tall" || Module<ActorMgr>.Self.GetActorInfo(ao.TmpltId).modelType == "Npc_Tall" || Module<ActorMgr>.Self.GetActorInfo(a.TmpltId).modelType == "Npc_Strong" || Module<ActorMgr>.Self.GetActorInfo(ao.TmpltId).modelType == "Npc_Strong");
                                if (isTall)
                                    animName += "_Tall";
                                a.StopAction(ACType.Animation, false);
                                if (a.TryDoAction(ACType.Animation, ACTAnimationPara.Construct(animName, null, null, true)))
                                {
                                    ao.StopAction(ACType.Animation, false);
                                    string modelType2 = Module<ActorMgr>.Self.GetActorInfo(ao.TmpltId).modelType;
                                    if (ao.TryDoAction(ACType.Animation, ACTAnimationPara.Construct(animName, null, EndAnimation, true)))
                                    {
                                        kissLocations.Add(ao.gamePos - a.gamePos); // add location for audio

                                        kissingSpouses.Add(a.InstanceId,currentTime); // for resetting animations
                                        kissingSpouses.Add(ao.InstanceId,currentTime);

                                        lastKiss = currentTime; // reset timer for kissing

                                        // start animating
                                        a.AddGravityEffector(turnOffGravity);
                                        a.StartInteraction(FullBodyBipedEffector.LeftHand, ao.GetInteractionObject(iKName), true);
                                        a.StartInteraction(FullBodyBipedEffector.RightHand, ao.GetInteractionObject(iKName), true);

                                        ao.AddGravityEffector(turnOffGravity);
                                        ao.StartInteraction(FullBodyBipedEffector.LeftHand, a.GetInteractionObject(iKName), true);
                                        ao.StartInteraction(FullBodyBipedEffector.RightHand, a.GetInteractionObject(iKName), true);

                                        break;
                                    }
                                    else
                                    {
                                        ResetOneSpouseAnimation(a);
                                        a.TryDoAction(ACType.Transfer, ACTransferPara.Construct(string.Empty, apos, arot));
                                    }
                                }
                            }
                            else
                            {
                                a.TryDoAction(ACType.Transfer, ACTransferPara.Construct(string.Empty, apos, arot));
                            }
                        }

                    }
                }
            }
        }
        public static Action EndAnimation = new Action(delegate ()
        {
            List<FavorObject> spouseList = GetSpouses();
            List<Actor> aSpouseList = new List<Actor>();

            foreach (FavorObject f in spouseList)
            {
                Actor a = ActorMgr.Self.Get(f.ID);
                if (a.InActiveScene)
                    aSpouseList.Add(a);
            }

            aSpouseList.Add(Module<Player>.Self.actor);


            foreach (Actor actor in aSpouseList)
            {
                if (kissingSpouses.ContainsKey(actor.InstanceId))
                    ResetOneSpouseAnimation(actor);
            }
        });

        private static void ResetOneSpouseAnimation(Actor actor)
        {
            if (actor == null)
                return;

            if (kissingSpouses.ContainsKey(actor.InstanceId))
                kissingSpouses.Remove(actor.InstanceId);

            actor.StopAction(ACType.Animation, false);
            actor.RemoveGravityEffector(turnOffGravity);
            actor.ResumeInteraction(FullBodyBipedEffector.LeftHand);
            actor.ResumeInteraction(FullBodyBipedEffector.RightHand);
        }

        private static bool isDebug = false;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "MarriageMod " : "") + str);
        }
    }
}