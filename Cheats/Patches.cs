using Harmony12;
using Pathea;
using Pathea.ActorNs;
using Pathea.AppearNs;
using Pathea.Behavior;
using Pathea.ConfigNs;
using Pathea.Conversations;
using Pathea.FavorSystemNs;
using Pathea.Festival;
using Pathea.HomeNs;
using Pathea.HomeViewerNs;
using Pathea.InputSolutionNs;
using Pathea.ItemDropNs;
using Pathea.ItemSystem;
using Pathea.MailNs;
using Pathea.MessageSystem;
using Pathea.Missions;
using Pathea.ModuleNs;
using Pathea.OptionNs;
using Pathea.StageNs;
using Pathea.UISystemNs;
using Pathea.UISystemNs.Grid;
using Pathea.UISystemNs.MainMenu.MissionUI;
using PatheaScriptExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static Harmony12.AccessTools;
using static Pathea.UISystemNs.MainMenu.MissionUI.MissionUICtr;

namespace Cheats
{
    public static partial class Main
    {
        static FieldRef<MissionUICtr, MissionType> curShowTypeByRef = FieldRefAccess<MissionUICtr, MissionType>("curShowType");
        static FieldRef<MissionManager, List<Mission>> missions_EndTypeByRef = FieldRefAccess<MissionManager, List<Mission>>("m_missions_End");

        [HarmonyPatch(typeof(MissionUICtr), "Start")]
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


        /*
                [HarmonyPatch(typeof(ActorInfo), "Instantiate")]
                static class ActorInfo_Instantiate_Patch
                {
                    static void Postfix(string ___model, GameObject __result)
                    {
                        if(___model == "Actor/Npc_Phyllis")
                        {
                            Mesh importedMesh = ObjImporter.ImportFile("G:/ga/My Time at Portia/modmanager/assets/Phyllis_LOD0.obj");
                            SkinnedMeshRenderer meshRenderer = __result.GetComponent<Actor>();

                            Texture2D texture = null;
                            byte[] fileData;

                            string filePath = "G:/ga/My Time at Portia/modmanager/assets/Phyllis2.png";

                            if (File.Exists(filePath))
                            {
                                fileData = File.ReadAllBytes(filePath);
                                texture = new Texture2D(1024, 1024);
                                texture.LoadRawTextureData(fileData);
                            }

                            meshRenderer.material.mainTexture = texture;
                    }
                }
            }
          */

        [HarmonyPatch(typeof(ActorEquip), "ApplyCloth")]
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

        [HarmonyPatch(typeof(Actor), "AddVisible")]
        static class Actor_Init_Patch
        {
            static void Postfix(Actor __instance, int ___instanceId, SkinnedMeshRenderer ___skinnedMeshRenderer)
            {
                if (!customTextures.ContainsKey(__instance.ActorName))
                    return;

                Texture texture = customTextures[__instance.ActorName];
                if(texture != null && ___skinnedMeshRenderer != null)
                    ___skinnedMeshRenderer.material.SetTexture("_MainTex", texture);

            }
        }
        
        [HarmonyPatch(typeof(ActorEquip), "ApplyCloth")]
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
                                    appearUnit.Smr.material.SetTexture("_MainTex", customTextures["Linda_Pants018"]);
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

        [HarmonyPatch(typeof(ItemBag), "InitItemBag", new Type[] { typeof(int),typeof(int)})]
        static class ItemBag_InitItemBag_Patch
        {
            static void Prefix(ItemBag __instance, ref int page, ref ItemTable[] ___itemTables)
            {
                //page = 10;
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

        [HarmonyPatch(typeof(FishBowlUnitViewer), "PlayerTarget_TriggerEvent")]
        static class FishBowlUnitViewer_PlayerTarget_TriggerEvent_Patch
        {
            static void Postfix(ActionType type, FishBowlUnitViewer __instance, FishBowlUnit ___fishBowlUnit)
            {
                if (type == ActionType.ActionAttack)
                {
                    ShowHunger(__instance, ___fishBowlUnit);
                }
            }
        }

        [HarmonyPatch(typeof(FishBowlUnitViewer), "FreshActionHint", new Type[] { })]
        static class FishBowlUnitViewer_FreshInteractHint_Patch
        {
            static bool Prefix(FishBowlUnitViewer __instance, FishBowlUnit ___fishBowlUnit)
            {
                ShowHunger(__instance, ___fishBowlUnit);
                return false;
            }
        }

        static void ShowHunger(FishBowlUnitViewer __instance, FishBowlUnit ___fishBowlUnit) {
            FieldRef<UnitViewer, PlayerTargetMultiAction> CurPlayerTargetRef = FieldRefAccess<UnitViewer, PlayerTargetMultiAction>("playerTarget");

            PlayerTargetMultiAction CurPlayerTarget = CurPlayerTargetRef(__instance);

            if (CurPlayerTarget == null)
                return;

            Dbgl("Showing Hunger");

            int count = ___fishBowlUnit.FishCount;
            int hungry = count;

            for (int i = 0; i < count; i++)
            {
                if (___fishBowlUnit.GetFish(i).isFull)
                {
                    hungry--;
                }
            }

            ItemObject curUseItem = Module<Player>.Self.bag.itemBar.GetCurUseItem();
            if (curUseItem != null && FishData.HasId(curUseItem.ItemDataId))
            {
                CurPlayerTarget.SetAction(ActionType.ActionAttack, TextMgr.GetStr(300315, -1), ActionTriggerMode.Normal);
            }
            else if (curUseItem != null && FishFeedItem.HasId(curUseItem.ItemDataId))
            {
                CurPlayerTarget.SetAction(ActionType.ActionAttack, TextMgr.GetStr(300317, -1) + " (Hungry: " + hungry + "/" + ___fishBowlUnit.FishCount + ")", ActionTriggerMode.Normal);
            }
            else
            {
                CurPlayerTarget.RemoveAction(ActionType.ActionAttack, ActionTriggerMode.Normal);
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