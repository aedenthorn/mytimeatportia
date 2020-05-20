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
using Pathea.RiderAdapterNs;
using Pathea.RiderNs;
using Pathea.ScenarioNs;
using Pathea.ScreenMaskNs;
using Pathea.StoreNs;
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

namespace SaveAnyTime
{
    public partial class Main
    {

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

            Dbgl("file exists " + filePath);
            Module<GameConfigModule>.Self.IgnoreCloseDoorAudio = true;

            Singleton<GameRunner>.Instance.Rest();
            Singleton<LoadingMask.Mgr>.Instance.Begin(null);
            yield return null;

            Dbgl("starting load routine");
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

            Dbgl("input solution: " +Module<InputSolutionModule>.Self.CurSolutionType + "");
            Module<InputSolutionModule>.Self.Pop();
            Module<InputSolutionModule>.Self.Push(SolutionType.Gaming);


            //Module<SleepModule>.Self.GetType().GetMethod("PlayerWakeUpAfterArchive", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Module<SleepModule>.Self, new object[] { });

            //Module<SleepModule>.Self.GetType().GetMethod("ShowSleepMask", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Module<SleepModule>.Self, new object[] { false });
            //MessageManager.Instance.Dispatch("WakeUpScreen", null, DispatchType.IMME, 2f);

            //Module<SleepModule>.Self.WakeUpScreenMaskFinishedEvent?.Invoke();

            Singleton<SleepTipMgr>.Self.SleepState(false);

            Dbgl("Checking DLC");

            // stuff that needs to be recreated after save

            GameDLCRewardsModule.Self.GetType().GetMethod("CheckAndOpenAllDlc", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Module<GameDLCRewardsModule>.Self, new object[] { });

            Dbgl("Checking Engagement");

            if (Module<EGMgr>.Self.IsEngagement())
            {
                Dbgl("Engagement is active");
                EGDate date = AccessTools.FieldRefAccess<EGMgr, EGDate>(Module<EGMgr>.Self, "mDate");
                GameDateTime dateBegin = AccessTools.FieldRefAccess<EGDate, GameDateTime>(date, "mBeginTimer");
                if (Module<TimeManager>.Self.DateTime > dateBegin)
                {
                    date.GetType().GetMethod("InitProjectMap", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(date, new object[] { });
                    Actor mActor = AccessTools.FieldRefAccess<EGDate, Actor>(date, "mActor");
                    AccessTools.FieldRefAccess<EGDate, Actor>(date, "mActor").SetBehaviorValue("EGActor", Module<Player>.Self.actor);
                    AccessTools.FieldRefAccess<EGDate, Actor>(date, "mActor").SetBehaviorValue("EGDate", EGData.GetDatePlace(AccessTools.FieldRefAccess<EGDate, int>(date, "mDateID")));
                    AccessTools.FieldRefAccess<EGDate, SharedInt>(date, "mForceValue") = mActor.GetBehaviorVariable<SharedInt>("EGForce");
                    AccessTools.FieldRefAccess<EGDate, SharedInt>(date, "mMoodValue") = mActor.GetBehaviorVariable<SharedInt>("EGMood");
                    AccessTools.FieldRefAccess<EGDate, SharedIntList>(date, "mEventCount") = mActor.GetBehaviorVariable<SharedIntList>("EGEventIDs");
                    AccessTools.FieldRefAccess<EGDate, List<EGRoot>>(date, "mRoots") = mActor.behavior.FindTasks<EGRoot>();
                    date.Start();
                    Dbgl("Engagement starts with " + mActor.ActorName);
                }
                else if (Module<TimeManager>.Self.DateTime > dateBegin - EGConst.Spawn_Hour_1)
                {
                    Dbgl("Less than one hour before engagement starts!");
                    date.GetType().GetMethod("InitProjectMap", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(date, new object[] { });
                    Actor mActor = AccessTools.FieldRefAccess<EGDate, Actor>(date, "mActor");
                    AccessTools.FieldRefAccess<EGDate, Actor>(date, "mActor").SetBehaviorValue("EGActor", Module<Player>.Self.actor);
                    AccessTools.FieldRefAccess<EGDate, Actor>(date, "mActor").SetBehaviorValue("EGDate", EGData.GetDatePlace(AccessTools.FieldRefAccess<EGDate, int>(date, "mDateID")));
                    AccessTools.FieldRefAccess<EGDate, SharedInt>(date, "mForceValue") = mActor.GetBehaviorVariable<SharedInt>("EGForce");
                    AccessTools.FieldRefAccess<EGDate, SharedInt>(date, "mMoodValue") = mActor.GetBehaviorVariable<SharedInt>("EGMood");
                    AccessTools.FieldRefAccess<EGDate, SharedIntList>(date, "mEventCount") = mActor.GetBehaviorVariable<SharedIntList>("EGEventIDs");
                    AccessTools.FieldRefAccess<EGDate, List<EGRoot>>(date, "mRoots") = mActor.behavior.FindTasks<EGRoot>();
                    Singleton<TipsMgr>.Instance.SendSystemTip(string.Format(TextMgr.GetStr(100507, -1), TextMgr.GetStr(AccessTools.FieldRefAccess<EGDate, int>(date, "mTipTypeID"), -1)), SystemTipType.warning);
                }
                else if (Module<TimeManager>.Self.DateTime > dateBegin - EGConst.Spawn_Hour_2)
                {
                    Dbgl("Less than two hours hour before engagement starts!");
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

            if (arg.IsMain)
            {
                Dbgl("Checking Ridables");
                Module<FarmModule>.Self.ForeachUnit(delegate (Unit unit, bool isFloor)
                {
                    if (unit != null && unit is RidableTamingUnit)
                    {
                        RidableTamingUnit ru = unit as RidableTamingUnit;
                        GameObject unitGameObjectByUnit = Module<FarmModule>.Self.GetUnitGameObjectByUnit(unit);
                        if (unitGameObjectByUnit == null)
                        {
                            return;
                        }
                        RidableTamingUnitViewer uv = (RidableTamingUnitViewer)unitGameObjectByUnit.GetComponentInChildren<UnitViewer>();

                        AccessTools.FieldRefAccess<RidableTamingUnitViewer, List<IRidable>>(uv, "ridableList").Clear();

                        typeof(RidableTamingUnitViewer).GetMethod("CreateShit", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(uv, new object[] { });
                        typeof(RidableTamingUnitViewer).GetMethod("CreateAllWorkableRidable", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(uv as RidableTamingUnitViewer, new object[] { });
                        typeof(RidableTamingUnitViewer).GetMethod("UpdateAllRidableInfo", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(uv as RidableTamingUnitViewer, new object[] { });

                    }
                });

                typeof(RidableModuleManager).GetMethod("InitIdGenerator", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Module<RidableModuleManager>.Self, new object[] { });

                Dictionary<int, RidableTransactionSaveData> rideDic = AccessTools.FieldRefAccess<RidableModuleManager, Dictionary<int, RidableTransactionSaveData>>(Module<RidableModuleManager>.Self, "ridableTransactionDataDic");
                int[] rideKeys = new int[rideDic.Count];
                rideDic.Keys.CopyTo(rideKeys, 0);
                foreach (int key in rideKeys)
                {
                    if (rideDic[key].RidableSource == RidableSource.NPC)
                    {
                        AccessTools.FieldRefAccess<RidableModuleManager, Dictionary<int, RidableTransactionSaveData>>(Module<RidableModuleManager>.Self, "ridableTransactionDataDic").Remove(key);
                    }
                }

                Scene sceneByName = SceneManager.GetSceneByName(arg.scenarioName);
                if (sceneByName.IsValid() && sceneByName.isLoaded)
                {
                    GameObject[] gos = sceneByName.GetRootGameObjects();
                    foreach (GameObject go in gos)
                    {
                        Component co = go.GetComponentInChildren(typeof(NpcsRidableManager));
                        if (co != null)
                        {
                            Dbgl("Got NpcsRidableManager");
                            (co as NpcsRidableManager).DestoryAllRidable();
                            AccessTools.FieldRefAccess<RidableFences, Dictionary<IRidable, RidableFence>>((co as NpcsRidableManager), "ridableDic").Clear();
                            typeof(NpcsRidableManager).GetMethod("AfterPlayerWakeUpEvent", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(co as NpcsRidableManager, new object[] { });
                        }
                    }
                }
            }

            Dbgl("Checking wishes");
            AccessTools.FieldRefAccess<DynamicWishManager, List<int>>(Module<DynamicWishManager>.Self, "hasTalkToday").AddRange(from it in AccessTools.FieldRefAccess<DynamicWishManager, List<DoubleInt>>(Module<DynamicWishManager>.Self, "curWishData") select it.id0);

            //Module<RidableModuleManager>.Self.InitNpcRidableBehaviourValue();

            // stuff that needs to be carried over but isnt, use meta file

            Dbgl("Loading Meta");
            string filePath = Path.Combine(GetSavesPath(), $"{lastLoadedSave.fileName}.xml");
            try
            {
                System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(SaveMeta));
                StreamReader file = new StreamReader($"{filePath}");
                SaveMeta save = (SaveMeta)reader.Deserialize(file);
                file.Close();

                Dbgl("Loading NPC Meta");

                foreach (NPCMeta npc in save.NPClist)
                {
                    Actor actor = Module<ActorMgr>.Self.Get(npc.id);
                    if (actor != null)
                    {
                        Module<ActorMgr>.Self.MoveToScenario(actor, npc.scene, VectorFromString(npc.pos));
                    }
                }

                Dbgl("Loading Ridable Meta");

                if (save.RideableList != null)
                {
                    foreach (RideableMeta r in save.RideableList)
                    {
                        IRidable rideable = Module<RidableModuleManager>.Self.GetRidable(r.id);
                        if (rideable == null)
                        {
                            Dbgl("null rideable " + r.id);

                            rideable = Module<RidableModuleManager>.Self.GetRidable(r.id);

                            continue;
                        }
                        Dbgl("got rideable " + r.id);
                        Actor actor = rideable.GetActor();
                        if (actor != null)
                        {
                            Dbgl("got rideable actor for " + rideable.GetNickName());
                            actor.gamePos = VectorFromString(r.pos);
                            actor.RefreshPos();
                        }
                        switch (r.state)
                        {
                            case "None":
                                rideable.SetRidableState(RidableState.None);
                                break;
                            case "Idle":
                                rideable.SetRidableState(RidableState.Idle);
                                break;
                            case "Ride":
                                if (rideable.BelongToPlayer)
                                {
                                    int otherNPCID = Module<EGMgr>.Self.GetEngagementStartNpcID();
                                    if (RideUtils.TestRideWithNpcID > 0)
                                    {
                                        otherNPCID = RideUtils.TestRideWithNpcID;
                                        RideUtils.TestRideWithNpcID = -1;
                                    }
                                    Module<Player>.Self.RideRidable(rideable, otherNPCID);
                                }
                                else if (rideable.GetBelongRider() is ActorRiderAdapter)
                                {
                                    Actor belongActor = (rideable.GetBelongRider() as ActorRiderAdapter).actor;
                                    RideController rideController = belongActor.RideController;
                                    rideController.RideOn(rideable);
                                }
                                break;
                            case "Follow":
                                rideable.SetRidableState(RidableState.Follow);
                                break;
                            case "Stay":
                                rideable.SetRidableState(RidableState.Stay);
                                break;
                        }
                    }
                }

                Dbgl("Loading Fishbowl Meta");

                if (save.FishBowlConsumeHour != -1)
                {
                    typeof(FishBowl).GetField("consumeHour", BindingFlags.NonPublic | BindingFlags.Static).SetValue(Module<FishBowl>.Self, save.FishBowlConsumeHour);
                }

                Dbgl("Loading Store Meta");

                AccessTools.FieldRefAccess<StoreManagerV40, float>(Module<StoreManagerV40>.Self, "curPriceIndex") = save.CurPriceIndex;

                if (save.StoreList != null)
                {
                    foreach (StoreMeta sMeta in save.StoreList)
                    {
                        Module<StoreManagerV40>.Self.GetStore(sMeta.id).recycleCount = sMeta.recycleCount;
                        Module<StoreManagerV40>.Self.GetStore(sMeta.id).ownMoney = sMeta.money;
                    }
                }

                Dbgl("Loading Weather Meta");

                if (save.WeatherState != -1)
                {
                    AccessTools.FieldRefAccess<WeatherModule, WeatherCtr>(Module<WeatherModule>.Self, "weatherCtr").SetWeather((WeatherState)save.WeatherState);
                }
            }
            catch (Exception ex)
            {
                Dbgl("Problem with meta file: " + ex);
            }
            isLoading = false;
        }

    }
}
