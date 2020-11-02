using Harmony12;
using Hont.ExMethod.Collection;
using NovaEnv;
using Pathea;
using Pathea.ACT;
using Pathea.ActorNs;
using Pathea.AimSystemNs;
using Pathea.AppearNs;
using Pathea.Behavior;
using Pathea.CameraSystemNs;
using Pathea.EffectNs;
using Pathea.EquipmentNs;
using Pathea.FavorSystemNs;
using Pathea.GameFlagNs;
using Pathea.HomeNs;
using Pathea.HomeViewerNs;
using Pathea.InputSolutionNs;
using Pathea.IntendNs;
using Pathea.ItemSystem;
using Pathea.LightMgr;
using Pathea.MiniGameNs.Fishing;
using Pathea.Missions;
using Pathea.ModuleNs;
using Pathea.NpcInstanceNs;
using Pathea.OptionNs;
using Pathea.RiderNs;
using Pathea.ScenarioNs;
using Pathea.StageNs;
using Pathea.SwitchNs;
using Pathea.UISystemNs;
using Pathea.UISystemNs.Grid;
using Pathea.UISystemNs.MainMenu.MissionUI;
using Pathea.WeatherNs;
using PatheaScriptExt;
using System;
using System.Collections.Generic;
using System.Reflection;
using TinyJson;
using UnityEngine;
using UnityEngine.UI;
using static Harmony12.AccessTools;
using static Pathea.UISystemNs.MainMenu.MissionUI.MissionUICtr;

namespace Cheats
{
    public static partial class Main
    {
        static FieldRef<MissionUICtr, MissionType> curShowTypeByRef = FieldRefAccess<MissionUICtr, MissionType>("curShowType");
        static FieldRef<MissionManager, List<Mission>> missions_EndTypeByRef = FieldRefAccess<MissionManager, List<Mission>>("m_missions_End");






        //[HarmonyPatch(typeof(MissionUICtr), "Start")]
        static class MissionUICtr_Start_Patch
        {
            static void Postfix(MissionUICtr __instance, GridPage ___missionTitleGrid)
            {
                void RestartMission(int index)
                {
                    MissionType curShowType = curShowTypeByRef(__instance);
                    List<MissionHistoryRecord> historyRecord = Module<MissionManager>.Self.HistoryRecord;
                    List<Mission> missionProgress = Module<MissionManager>.Self.GetMissionRunning();
                    List<MissionHistoryRecord> missionNormalDone = new List<MissionHistoryRecord>();
                    for (int i = 0; i < historyRecord.Count; i++)
                    {
                        if (!historyRecord[i].IsOrderMission)
                        {
                            missionNormalDone.Add(historyRecord[i]);
                        }
                    }
                    MissionBaseInfo m = null;
                    if (curShowType == MissionUICtr.MissionType.progress && index < missionProgress.Count && index >= 0)
                    {
                        Dbgl("clicked progress mission");
                        m = MissionManager.GetmissionBaseInfo(missionProgress[index].InstanceID);
                    }
                    else if (curShowType == MissionUICtr.MissionType.normalDone && index < missionNormalDone.Count && index >= 0)
                    {
                        m = MissionManager.GetmissionBaseInfo(missionNormalDone[index].InstanceId);
                        Dbgl("clicked done mission "+ missionNormalDone[index].InstanceId+" "+ missionNormalDone[index].MissionNo);

                        List<Mission> doneMissions = missions_EndTypeByRef(MissionManager.GetInstance);

                        Dbgl("done missions before: "+doneMissions.Count);

                        doneMissions.RemoveAll((Mission it) => it.InstanceID == m.InstanceID);
                        MissionManager.GetInstance.HistoryRecord.RemoveAll((MissionHistoryRecord hr) => hr.InstanceId == m.InstanceID);
                        missionNormalDone.Remove(missionNormalDone[index]);
                        Dbgl("done missions after: " + doneMissions.Count);

                        MissionManager.GetInstance.GetType().GetField("m_missions_End", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(MissionManager.Self, doneMissions);
                        Dbgl("real done missions after: " + missions_EndTypeByRef(MissionManager.Self).Count);

                    }
                    if (m != null)
                    {
                        Dbgl("clicked mission not null id:"+m.InstanceID +" number:"+m.MissionNO+" name: "+TextMgr.GetStr(m.MissionNameId,-1));
                        return;
                        //MethodInfo dynMethod = MissionManager.GetInstance.GetType().GetMethod("InitSaveData", BindingFlags.NonPublic | BindingFlags.Instance);
                        //dynMethod.Invoke(MissionManager.GetInstance, new object[] { });

                        MissionManager.GetInstance.UpgradeRecord();
                        MissionManager.GetInstance.RefreshAllNpcMissionIcon();
                        MissionManager.GetInstance.DispatcherMissionTargetRefresh();
                        MissionManager.GetInstance.FreshTrace();

                        Mission mi = new Mission();
                        mi.InitFromBaseInfo(m);
                        MissionManager.GetInstance.DeliverMission(mi);
                    }
                }
                ___missionTitleGrid.OnMiddleClick += RestartMission;
            }
        }


        //[HarmonyPatch(typeof(LightManager), "OpenLight")]
        static class rmm2_Patch
        {
            static void Postfix(int groupID)
            {
                GameObject[] lightsObject = LightGroupData.GetLightsObject(groupID);
                for (int i = 0; i < lightsObject.Length; i++)
                {
                    Light componentInChildren = lightsObject[i].GetComponentInChildren<Light>();
                    if (componentInChildren)
                    {
                        componentInChildren.shadows = LightShadows.Hard;
                        componentInChildren.shadowStrength = 1f;
                        componentInChildren.shadowBias = 0.05f;
                        componentInChildren.shadowNormalBias = 0.4f;
                        componentInChildren.shadowNearPlane = 0.2f;
                        componentInChildren.shadowResolution = UnityEngine.Rendering.LightShadowResolution.VeryHigh;
                        componentInChildren.shadowCustomResolution = -1;
                    }
                    MatSwitch componentInChildren2 = lightsObject[i].GetComponentInChildren<MatSwitch>();
                    if (componentInChildren2)
                    {
                        Material[] nightMats = FieldRefAccess<MatSwitch, Material[]>(componentInChildren2, "nightMats");
                        for(int j = 0; j < nightMats.Length; j++)
                        {
                            //Color c = nightMats[j].GetColor("_Emissions");
                            //nightMats[j].SetColor("_Emissions", c * 2f);
                        }
                    }
                    EffectSwitch componentInChildren3 = lightsObject[i].GetComponentInChildren<EffectSwitch>();
                    if (componentInChildren3)
                    {
                    }
                }
            }
        }


                //[HarmonyPatch(typeof(PatrolArea), "OnStart")]
        static class PatrolArea_Patch
        {
            static void Prefix(PatrolArea __instance)
            {
                //Dbgl($"npc name: {__instance.actor.Value.ActorName}");
                if (__instance.actor.Value.InstanceId != 4000044)
                    return;

                int _ridableId = Module<RidableModuleManager>.Self.GetNpcRidableUID(__instance.actor.Value.InstanceId);
                Dbgl($"ridable id: {_ridableId}");

                foreach (IRidable r in AccessTools.FieldRefAccess<RidableModuleManager, List<IRidable>>(Module<RidableModuleManager>.Self, "allRidable"))
                {
                    Dbgl($"ridable id: {r.UID} name: {r.GetNickName()}, owner: {r.GetBelongRider().GetName()}");
                }

                IRidable _ridable = Module<RidableModuleManager>.Self.GetRidable(_ridableId);

                if (_ridable == null)
                {
                    Dbgl($"ridable is null");
                    return;
                }
                Dbgl($"PatrolArea OnStart: {_ridableId} {_ridable.GetNickName()}");
                return;
                foreach (KeyValuePair<int, RidableTransactionSaveData> kvp in AccessTools.FieldRefAccess<RidableModuleManager, Dictionary<int, RidableTransactionSaveData>>(Module<RidableModuleManager>.Self, "ridableTransactionDataDic"))
                {
                    int id = kvp.Key;
                    Dbgl($"got dic entry {id} {kvp.Value.BelongActorID} ActorName: {__instance.actor.Value.ActorName} ActorId: {__instance.actor.Value.InstanceId}");
                }

            }
        }


        //[HarmonyPatch(typeof(Player), "TriggerAction")]
        static class ScenarioModule_PostLoad_Patch
        {
            static void Postfix(ActionType type, ActionTriggerMode mode, ref bool __result, Player __instance, Intend ___intend)
            {
                if (type == ActionType.ActionAttack && Singleton<GameFlag>.Instance.Gaming && __instance.IsActionRunning(ACType.Jump) && ___intend.DoAction(IntendBtn.Left))
                {
                    Dbgl(type + " can " + mode);
                    __result = true;
                }
            }
        }


        //[HarmonyPatch(typeof(ActorEquip), "ApplyCloth")]
        static class ActorEquip_ApplyCloth_Patch
        {
            static void Prefix(ActorEquip __instance, string[] ___nudeAppearUnits, string[] ___equipAppearUnits, GameObject clothRoot, bool showHat, AppearData ___appearData, string ___tattooPath)
            {
                return;
                List<AppearUnit> list = new List<AppearUnit>();
                AppearUnit tattooTarget = null;
                for (int i = 0; i < 5; i++)
                {
                    if (___nudeAppearUnits.Length > i && ___nudeAppearUnits[i] != null)
                    {
                        AppearUnit appearUnit = null;
                        if (i < ___equipAppearUnits.Length && !string.IsNullOrEmpty(___equipAppearUnits[i]) && ((showHat && Singleton<OptionsMgr>.Self.ShowHat) || i != 1))
                        {
                            AppearUnit appearUnit2 = Singleton<ResMgr>.Instance.LoadSyncByType<AppearUnit>(AssetType.Appear, ___equipAppearUnits[i]);
                            if (appearUnit2 != null)
                            {
                                appearUnit = appearUnit2;
                            }
                        }
                        else
                        {
                            appearUnit = Singleton<ResMgr>.Instance.LoadSyncByType<AppearUnit>(AssetType.Appear, ___nudeAppearUnits[i]);
                        }
                        if (appearUnit != null)
                        {
                            list.Add(appearUnit);
                            if (i == 0)
                            {
                                tattooTarget = appearUnit;
                            }
                        }
                    }
                }
                if (clothRoot == null)
                {
                    clothRoot = __instance.ClothRoot;
                }
                AppearTarget.Instance.SetRoot(clothRoot);
                AppearTarget.Instance.BuildMesh(list, ___appearData, tattooTarget, ___tattooPath);
                MethodInfo dynMethod = __instance.GetType().GetMethod("ApplyDyboneConfigs",BindingFlags.NonPublic | BindingFlags.Instance);
                dynMethod.Invoke(__instance, new object[] { AppearTarget.Instance.BoneDic });
            }
        }

        //[HarmonyPatch(typeof(Actor), "AddVisible")]
        static class Actor_Init_Patch
        {
            static void Postfix(Actor __instance, int ___instanceId, SkinnedMeshRenderer ___skinnedMeshRenderer)
            {
                if (!customTextures.ContainsKey(__instance.InstanceId))
                    return;

                Texture texture = customTextures[__instance.InstanceId];
                if(texture != null && ___skinnedMeshRenderer != null)
                    ___skinnedMeshRenderer.material.SetTexture("_MainTex", texture);

            }
        }
        
        //[HarmonyPatch(typeof(ActorEquip), "ApplyCloth")]
        static class AppearTarget_BuildMesh_Patch
        {
            static bool Prefix(GameObject clothRoot, bool showHat, string[] ___nudeAppearUnits, string[] ___equipAppearUnits, AppearData ___appearData, string ___tattooPath, ActorEquip __instance)
            {
                List<AppearUnit> list = new List<AppearUnit>();
                AppearUnit tattooTarget = null;
                for (int i = 0; i < 5; i++)
                {
                    if (___nudeAppearUnits.Length > i && ___nudeAppearUnits[i] != null)
                    {
                        AppearUnit appearUnit = null;
                        if (i < ___equipAppearUnits.Length && !string.IsNullOrEmpty(___equipAppearUnits[i]) && ((showHat && Singleton<OptionsMgr>.Self.ShowHat) || i != 1))
                        {
                            AppearUnit appearUnit2 = Singleton<ResMgr>.Instance.LoadSyncByType<AppearUnit>(AssetType.Appear, ___equipAppearUnits[i]);
                            if (appearUnit2 != null)
                            {
                                appearUnit = appearUnit2;
                                if (___equipAppearUnits[i].Contains("Linda_Pants018"))
                                {
                                    //appearUnit.Smr.material.SetTexture("_MainTex", customTextures["Linda_Pants018"]);
                                }
                            }
                        }
                        else
                        {
                            appearUnit = Singleton<ResMgr>.Instance.LoadSyncByType<AppearUnit>(AssetType.Appear, ___nudeAppearUnits[i]);
                        }
                        if (appearUnit != null)
                        {
                            list.Add(appearUnit);
                            if (i == 0)
                            {
                                tattooTarget = appearUnit;
                            }
                        }
                    }
                }
                if (clothRoot == null)
                {
                    clothRoot = __instance.ClothRoot;
                }
                AppearTarget.Instance.SetRoot(clothRoot);
                AppearTarget.Instance.BuildMesh(list, ___appearData, tattooTarget, ___tattooPath);

                MethodInfo dynMethod = __instance.GetType().GetMethod("ApplyDyboneConfigs", BindingFlags.NonPublic | BindingFlags.Instance);
                dynMethod.Invoke(__instance, new object[] { AppearTarget.Instance.BoneDic });

                return false;
            }
        }

        //[HarmonyPatch(typeof(ItemBag), "InitItemBag", new Type[] { typeof(int),typeof(int)})]
        static class ItemBag_InitItemBag_Patch
        {
            static void Prefix(ItemBag __instance, ref int page, ref ItemTable[] ___itemTables)
            {
                //page = 10;
            }
        }

        //[HarmonyPatch(typeof(GamingSolution), "Init")]
        static class GamingSolution_Patch
        {
            static void Prefix()
            {
                Dbgl("GamingSolution Init " + Environment.StackTrace);
            }
        }
        
        //[HarmonyPatch(typeof(ItemTable))]
        //[HarmonyPatch(new Type[] { typeof(int), typeof(bool) })]
        static class ItemTable_Patch
        {
            static void Prefix(int size, ref bool isDefaultCount)
            {
                if (size == 400)
                    isDefaultCount = true;
            }
        }

        //[HarmonyPatch(typeof(PlayerItemBarCtr), "Update")]
        static class ItemBar_Patch
        {
            static void Prefix(PlayerItemBarCtr __instance)
            {
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    for (int index = 0; index < 8; index++)
                    {
                        ItemObject itemObject = Module<Player>.Self.bag.itemBar.itemBarItems[index];
                        ItemObject itemObj = Module<Player>.Self.bag.GetItems(0).GetItemObj(index);
                        if (itemObject != null && itemObj != null && itemObject.ItemBase.ID == itemObj.ItemBase.ID && itemObj.RemainCapacity() != 0)
                        {
                            int num2 = itemObj.RemainCapacity();
                            if (itemObject.Number <= num2)
                            {
                                itemObj.ChangeNumber(itemObject.Number);
                                Module<Player>.Self.bag.itemBar.SetItemObject(null, index);
                            }
                            else
                            {
                                itemObj.ChangeNumber(num2);
                                itemObject.ChangeNumber(-num2);
                            }
                        }
                        else
                        {
                            Module<Player>.Self.bag.BagExchangeItemBar(index, index, 0);
                        }
                    }

                    MethodInfo dynMethod = __instance.GetType().GetMethod("Unequip", BindingFlags.NonPublic | BindingFlags.Instance);
                    dynMethod.Invoke(__instance, new object[] { });
                }
                else if (Input.GetKeyDown(KeyCode.O))
                {
                    StorageViewer sv = new StorageViewer();
                    FieldRef<StorageViewer,StorageUnit> suRef = FieldRefAccess<StorageViewer, StorageUnit>("storageUnit");
                    suRef(sv) = StorageUnit.GetStorageByGlobalIndex(0);

                    MethodInfo dynMethod = sv.GetType().GetMethod("InteractStorage", BindingFlags.NonPublic | BindingFlags.Instance);
                    dynMethod.Invoke(sv, new object[] { });
                }
            }
        }
        /*
        [HarmonyPatch(typeof(PlayerInputCreator), "Init")]
        static class PlayerInputCreator_Patch
        {
            static void Postfix()
            {
                Dbgl("overriding scanner input!");
                Module<InputSolutionModule>.Self.Set(SolutionType.Releaver, new EnhancedScannerSolution());
            }
        }
        
        [HarmonyPatch(typeof(TreasureRevealer), "EnterAimMode")]
        static class TreasureRevealer_Patch
        {
            static void Postfix()
            {
                Dbgl("overriding scanner input!");
                CameraManager.Instance.PopStackActiveController(true, new object[0]);
                CameraManager.Instance.PushStackActiveController("AcrossShoulderCamera", true, new object[] { });
            }
        }
        */


        //[HarmonyPatch(typeof(WholeMapViewer), "SetMarkIcon")]
        static class SetMarkIcon_Patch
        {
            static bool Prefix(WholeMapViewer __instance, Image iconImage, Image ___NowPlayingMap, bool ___forceWorldMap)
            {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    Vector3 vector = ___NowPlayingMap.rectTransform.InverseTransformPoint(iconImage.rectTransform.position);
                    MethodInfo dynMethod = __instance.GetType().GetMethod("ReverseConvertGamePosFromGameMap", BindingFlags.NonPublic | BindingFlags.Instance);
                    Vector3 v3 = (Vector3)dynMethod.Invoke(__instance, new object[] { new Vector3(vector.x, vector.y, 0f) });
                    v3.z = 100f;

                    Module<Player>.Self.GamePos = v3;

                    //Module<Player>.Self.actor.RequestForceMove(new Vector3(0,0,-1000f));

                    return false;
                }
                return true;
            }
        }


        //[HarmonyPatch(typeof(NpcRelationModule.RelationFlagTime), "Dispose")]
        static class NpcRelationModule_Patch
        {
            static void Prefix(NpcRelationModule.RelationFlagTime __instance)
            {
                Dbgl(__instance.selfID+" " +__instance.otherID+" " + Environment.StackTrace);
            }
        }

        //[HarmonyPatch(typeof(FavorObject), "GainFavorValue")]
        static class GainFavorValue_Patch
        {
            static void Prefix(FavorObject __instance)
            {
                Dbgl(__instance.ID.ToString());
            }
        }
        //[HarmonyPatch(typeof(Stage), "InitStage", new Type[] { })]
        static class Stage_InitStage_Patch
        {
            static bool Prefix(Stage __instance, ref List<StageItem> ___orderList)
            {
                PropertyInfo property = typeof(Stage).GetProperty("CurDay", BindingFlags.Instance);
                property.SetValue(__instance,Module<TimeManager>.Self.Days, null);

                MethodInfo dynMethod = __instance.GetType().GetMethod("ClearDataButMaxAccept", BindingFlags.NonPublic | BindingFlags.Instance);
                dynMethod.Invoke(__instance, new object[] { });

                List<StageItemData> list = StageItemData.List;
                for (int i = 0; i < list.Count; i++)
                {
                    StageItem stageItem = new StageItem();
                    stageItem.Convert(list[i]);
                    ___orderList.Add(stageItem);
                }

                List<TypePriceData> list2 = TypePriceData.List;
                int num2 = 0;
                List<IdCount> list3 = new List<IdCount>();
                for (int j = 0; j < list2.Count; j++)
                {
                    TypePriceData typePriceData = list2[j];
                    if (typePriceData.NumLimit != null && typePriceData.NumLimit.Length > 0 && typePriceData.NumLimit[0] > 0)
                    {
                        ___orderList[num2].SetPriceType(typePriceData.Type);
                        list3.Add(typePriceData.Type, 1);
                        num2++;
                    }
                    if (num2 >= list.Count)
                    {
                        break;
                    }
                }
                num2 = 0;
                list2 = GameUtils.RandomSortList<TypePriceData>(list2);
                foreach (StageItem stageItem2 in ___orderList)
                {
                    if (stageItem2.PriceType == 0)
                    {
                        TypePriceData typePriceData2 = list2[num2 % list2.Count];
                        stageItem2.SetPriceType(typePriceData2.Type);
                        num2++;
                    }
                }

                return false;
            }
        }

    }
}