using Harmony12;
using Hont.ExMethod.Collection;
using Pathea;
using Pathea.CompoundSystem;
using Pathea.HomeNs;
using Pathea.HomeViewerNs;
using Pathea.InputSolutionNs;
using Pathea.ItemSystem;
using Pathea.ModuleNs;
using Pathea.UISystemNs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace CookingMachine
{
    public static class Main
    {
        public static bool enabled;
        private static bool isDebug = true;
        private static List<int> crash;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "CookingMachine " : "") + str);
        }
        public static Settings settings { get; private set; }

        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnUpdate = OnUpdate;
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

        }


        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {

        }


        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }

        static void OnUpdate(UnityModManager.ModEntry modEntry, float dt)
        {

        }

        [HarmonyPatch(typeof(CompoundManager), "GetAllCompoundItems", new Type[] { typeof(bool)})]
        static class AutomataMachineMenuCtr_1
        {
            static void Postfix(ref CompoundItem[] __result)
            {
                int start = 5000;
                int start2 = 5001000;

                List<CompoundItem> newList = __result.ToList();
                //List<CompoundItem> allFoods = newList.FindAll((i) => i.m_plugInType == 4);
                List<CookBookData> cookList = Module<CookBookMgr>.Self.CookList;
                if (cookList == null || __result == null || __result.Length == 0)
                {
                    Dbgl("missing list");
                    return;
                }
                Dbgl("getting cid");

                SqliteDataReader reader = LocalDb.cur.ReadFullTable("Synthesis_table ");
                var sourceData = DbReader.Read<CompoundItemData>(reader, 20);

                CompoundItemData cidd = sourceData.Find((CompoundItemData c) => c.ItemID == 2000046);

                Dbgl("getting list");
                foreach (CookBookData food in cookList)
                {
                    if (food != null && food.like)
                    {
                        Dbgl("got food");
                        cidd.NameID = Module<ItemDataMgr>.Self.GetItemNameId(food.foodId);
                        cidd.ItemID = food.foodId;
                        Dbgl("food " + food.foodId);
                        Dbgl("menu list length " + food.menuList.Count);
                        foreach (CookMatData cmd in food.menuList)
                        {
                            Dbgl("matdic length " + cmd.matDic.Count);
                            cidd.ID = start++;
                            cidd.BookId = start2++;
                            cidd.RequireItem1 = cmd.matDic.Keys.ToList()[0];
                            cidd.RequireItemNum1 = cmd.matDic[cmd.matDic.Keys.ToList()[0]];
                            Dbgl($"item1 {cidd.RequireItem1} count {cidd.RequireItemNum1}");
                            if (cmd.matDic.Count > 1)
                            {
                                cidd.RequireItem2 = cmd.matDic.Keys.ToList()[1];
                                cidd.RequireItemNum2 = cmd.matDic[cmd.matDic.Keys.ToList()[1]];
                                Dbgl($"item2 {cidd.RequireItem2} count {cidd.RequireItemNum2}");
                                if (cmd.matDic.Count > 2)
                                {
                                    cidd.RequireItem3 = cmd.matDic.Keys.ToList()[2];
                                    cidd.RequireItemNum3 = cmd.matDic[cmd.matDic.Keys.ToList()[2]];
                                    Dbgl($"item3 {cidd.RequireItem3} count {cidd.RequireItemNum3}");
                                }
                                else
                                {
                                    cidd.RequireItem3 = 0;
                                    cidd.RequireItemNum3 = 0;
                                }
                            }
                            else
                            {
                                cidd.RequireItem2 = 0;
                                cidd.RequireItemNum2 = 0;
                            }
                            newList.Add(new CompoundItem(cidd));
                        }
                    }
                }
                Dbgl("list length" + __result.Length);
                __result = newList.ToArray();
                Dbgl("list length" + __result.Length);
            }
        }
        
        [HarmonyPatch(typeof(AutomataMachiePlace), "Handler")]
        static class AutomataMachiePlace_1
        {
            static void Prefix(ActionType type, int ___plugType)
            {
                if(___plugType == 4)
                {
                    if (type == ActionType.ActionRoll)
                    {
                        UIStateMgr.Instance.ChangeStateByType(UIStateMgr.StateType.CookMachine, false, null);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(AutomataMachiePlace), "FreshItemShow", new Type[] { typeof(int) })]
        static class AutomataMachiePlace_2
        {
            static void Postfix(AutomataMachiePlace __instance, int ___plugType)
            {
                if(___plugType == 4)
                {
                    PlayerTargetMultiAction CurPlayerTarget = (PlayerTargetMultiAction)typeof(UnitViewer).GetProperty("CurPlayerTarget", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance, null);
                    CurPlayerTarget.SetAction(ActionType.ActionRoll, 101174, ActionTriggerMode.Normal);
                }
            }
        }

        //[HarmonyPatch(typeof(CompoundManager), "InitCompoundData")]
        static class CompoundManager_GetSourceData
        {
            static void Postfix(ref List<CompoundItemData> ___sourceData, ref CompoundTable[] ___m_compoundTable)
            {
                if (!enabled)
                    return;

                CompoundItemData cidd = ___sourceData.Find((CompoundItemData c) => c.ItemID == 2000046);

                int start = 5000;
                int start2 = 5001000;
                List<CookMenuData> datas = Singleton<CookMenuMgr>.Instance.DataList;
                var TagList = DbReader.Read<CookItemtarget>(LocalDb.cur.ReadFullTable("Cook_TagList"), 20);
                foreach (CookMenuData data in datas)
                {
                    try 
                    {
                        if (data.Mats.Count > 0)
                        {
                            CompoundItemData cid = cidd;
                            cid.NameID = Module<ItemDataMgr>.Self.GetItemNameId(data.Food);
                            cid.ItemID = data.Food;
                            List<CookMenuData.Mat> mats = new List<CookMenuData.Mat>();
                            if (data.Mats[0].ID > 200)
                            {
                                mats.Add(data.Mats[0]);
                            }
                            else
                            {

                                List<CookItemtarget> items = TagList.FindAll((t) => t.Tag.Contains(data.Mats[0].ID));
                                foreach (CookItemtarget item in items)
                                {
                                    mats.Add(new CookMenuData.Mat()
                                    {
                                        ID = item.ID,
                                        Num = data.Mats[0].Num
                                    });
                                }
                            }
                            foreach (CookMenuData.Mat mat in mats)
                            {
                                cid.RequireItem1 = mat.ID;
                                cid.RequireItemNum1 = data.Mats[0].Num;
                                if (data.Mats.Count > 1)
                                {
                                    List<CookMenuData.Mat> mats2 = new List<CookMenuData.Mat>();
                                    if (data.Mats[1].ID > 200)
                                    {
                                        mats2.Add(data.Mats[1]);
                                    }
                                    else
                                    {
                                        List<CookItemtarget> items = TagList.FindAll((t) => t.Tag.Contains(data.Mats[1].ID) );
                                        foreach(CookItemtarget item in items)
                                        {
                                            mats2.Add(new CookMenuData.Mat()
                                            {
                                                ID = item.ID,
                                                Num = data.Mats[1].Num
                                            });
                                        }
                                    }
                                    foreach (CookMenuData.Mat mat2 in mats2)
                                    {
                                        cid.RequireItem2 = mat2.ID;
                                        cid.RequireItemNum2 = data.Mats[1].Num;
                                        if (data.Mats.Count > 2)
                                        {
                                            List<CookMenuData.Mat> mats3 = new List<CookMenuData.Mat>();
                                            if (data.Mats[2].ID > 200)
                                            {
                                                mats3.Add(data.Mats[2]);
                                            }
                                            else
                                            {
                                                List<CookItemtarget> items = TagList.FindAll((t) => t.Tag.Contains(data.Mats[2].ID));
                                                foreach (CookItemtarget item in items)
                                                {
                                                    mats3.Add(new CookMenuData.Mat()
                                                    {
                                                        ID = item.ID,
                                                        Num = data.Mats[2].Num
                                                    });
                                                }
                                            }
                                            foreach (CookMenuData.Mat mat3 in mats3)
                                            {
                                                cid.RequireItem3 = mat3.ID;
                                                cid.RequireItemNum3 = data.Mats[2].Num;

                                                cid.ID = start++;
                                                cid.BookId = start2++;
                                                CompoundItem ci = new CompoundItem(cid);
                                                ___m_compoundTable[(int)cidd.CompoundType].AddItem(ci);
                                            }
                                        }
                                        else
                                        {
                                            cid.ID = start++;
                                            cid.BookId = start2++;
                                            CompoundItem ci = new CompoundItem(cid);
                                            ___m_compoundTable[(int)cidd.CompoundType].AddItem(ci);
                                        }
                                    }
                                }
                                else
                                {
                                    cid.ID = start++;
                                    cid.BookId = start2++;
                                    CompoundItem ci = new CompoundItem(cid);
                                    ___m_compoundTable[(int)cidd.CompoundType].AddItem(ci);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Dbgl("exception: " + ex);
                    }
                }
            }

        }
    }
}