using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony12;
using Pathea;
using Pathea.AchievementNs;
using Pathea.AudioNs;
using Pathea.CompoundSystem;
using Pathea.EffectNs;
using Pathea.FarmFactoryNs;
using Pathea.HomeNs;
using Pathea.HomeViewerNs;
using Pathea.InputSolutionNs;
using Pathea.ItemBoxNs;
using Pathea.ItemDropNs;
using Pathea.ItemSystem;
using Pathea.MessageSystem;
using Pathea.ModuleNs;
using Pathea.SpawnNs;
using Pathea.TagNs;
using Pathea.UISystemNs;
using Pathea.UISystemNs.Grid;
using Pathea.UISystemNs.MainMenu.PackageUI;
using Pathea.UISystemNs.UIBase;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityModManagerNet;
using static Harmony12.AccessTools;

namespace InventoryUIAddons
{
    public class Main
    {
        private static readonly bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log($"{(pref ? typeof(Main).Namespace : "")} {str}");
        }

        public static UnityModManager.ModEntry context;

        public static Settings settings { get; private set; }
        public static bool enabled;

        public static bool IsSplitHalf;
        public static bool IsSplitOne;
        public static bool IsUseForced;

        private static void Load(UnityModManager.ModEntry modEntry)
        {
            context = modEntry;
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnToggle = OnToggle;

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
            GUILayout.Label("<b>Take All Key:</b>", new GUILayoutOption[0]);
            settings.TakeAllKey = GUILayout.TextField(settings.TakeAllKey, new GUILayoutOption[0]);
            GUILayout.Space(10);
            GUILayout.Label("<b>Grab Half Mod Key:</b>", new GUILayoutOption[0]);
            settings.GrabHalfModKey = GUILayout.TextField(settings.GrabHalfModKey, new GUILayoutOption[0]);
            GUILayout.Space(10);
            GUILayout.Label("<b>Grab One Mod Key:</b>", new GUILayoutOption[0]);
            settings.GrabOneModKey = GUILayout.TextField(settings.GrabOneModKey, new GUILayoutOption[0]);
            GUILayout.Space(10);
            settings.StoreToToolbarFirst = GUILayout.Toggle(settings.StoreToToolbarFirst, "Store To Toolbar First", new GUILayoutOption[0]);
            GUILayout.Space(20);
        }
        static bool KeyDown(string key)
        {
            try
            {
                return (Input.GetKeyDown(key));
            }
            catch
            {
                return false;
            }
        }

        [HarmonyPatch(typeof(ItemBag), "AddItem", new Type[] { typeof(ItemObject), typeof(AddItemMode) })]
        public static class ItemBag_AddItem_Patch
        {
            public static bool Prefix(ItemBag __instance, ref bool __result, ItemTable[] ___itemTables, ItemObject item, AddItemMode addItemMode)
            {
                if (!enabled || addItemMode != AddItemMode.Default || settings.StoreToToolbarFirst)
                    return true;

                int amountLeft = item.Number;

                
                // bag add to stack

                for (int l = 0; l < ___itemTables.Length; l++)
                {
                    amountLeft = ___itemTables[l].AddItemCount(item.ItemBase.ID, amountLeft);
                    if (amountLeft == 0)
                    {
                        __result = true;
                        return false;
                    }
                    item.DeleteNumber(item.Number - amountLeft);
                }


                // bar add to stack

                amountLeft = __instance.itemBar.AddItemCount(item.ItemBase.ID, amountLeft);
                if (amountLeft == 0)
                {
                    __result = true;
                    return false;
                }
                item.DeleteNumber(item.Number - amountLeft);


                // bag add new

                for (int m = 0; m < ___itemTables.Length; m++)
                {
                    bool flag3 = ___itemTables[m].AddItemObject(item);
                    if (flag3)
                    {
                        __result = true;
                        return false;
                    }
                }


                // bar add new

                List<int> free2 = __instance.itemBar.GetFree();
                if (free2.Count > 0)
                {
                    __instance.itemBar.SetItemObject(item, free2[0]);
                    __result = true;
                    return false;
                }

                AccessTools.Method(typeof(ItemBag), "DropOverFlowItem").Invoke(__instance, new object[] { item });
                return false;
            }
        }
        
        [HarmonyPatch(typeof(PackageUIBase), "Update")]
        public static class PackageUIBase_Update_Patch
        {
            public static void Postfix(PackageUIBase __instance)
            {
                if (!enabled || !(__instance is StoreageUIBase) || !KeyDown(settings.TakeAllKey))
                    return;
                StoreageUIBase ui = __instance as StoreageUIBase;
                var m = AccessTools.Method(typeof(StoreageUIBase), "BackFromStorage");
                for (int i = 0; i < ui.storeageSize; i++)
                {
                    if(ui.Storeage.GetItemObj(i) != null)
                        m.Invoke(__instance, new object[] { i });
                }
                ui.FreshCurpageItem();
                ui.playerItemBar.FreshItem();
                ui.FreshStoreage();
            }
        }

        [HarmonyPatch(typeof(PackageUIBase), "OnEnable")]
        public static class PackageUIBase_OnEnable_Patch
        {
            public static void Postfix(ref Image ___dragItem)
            {
                if (!enabled)
                    return; 
                MessageManager.Instance.Subscribe("UIOtherPackageSplitOne", new Action<object[]>(MouseSplitOne));
                MessageManager.Instance.Subscribe("UIOtherPackageSplitHalf", new Action<object[]>(MouseSplitHalf));
                MessageManager.Instance.Subscribe("UIOtherPackageUseItem", new Action<object[]>(MouseUseItem));
            }
        }
        [HarmonyPatch(typeof(PackageUIBase), "OnDisable")]
        public static class PackageUIBase_OnDisable_Patch
        {
            public static void Postfix()
            {
                if (!enabled)
                    return;
                MessageManager.Instance.Unsubscribe("UIOtherPackageSplitOne", new Action<object[]>(MouseSplitOne));
                MessageManager.Instance.Unsubscribe("UIOtherPackageSplitHalf", new Action<object[]>(MouseSplitHalf));
                MessageManager.Instance.Unsubscribe("UIOtherPackageUseItem", new Action<object[]>(MouseUseItem));
            }
        }
        [HarmonyPatch(typeof(PackageUISolution), "Update")]
        public static class PackageUISolution_Update_Patch
        {
            public static void Postfix(PackageInteractInput ___packageInteract)
            {
                if (!enabled || Module<InputSolutionModule>.Self.CurSolutionType == SolutionType.ColorConfig)
                    return;
                MessageManager.Instance.Dispatch("UIOtherPackageSplitOne", new object[] { (___packageInteract as PackageInteractInputExtended).packageSplitOne.IsPressed }, DispatchType.IMME, 2f);
                MessageManager.Instance.Dispatch("UIOtherPackageSplitHalf", new object[] { (___packageInteract as PackageInteractInputExtended).packageSplitHalf.IsPressed }, DispatchType.IMME, 2f);
                MessageManager.Instance.Dispatch("UIOtherPackageUseItem", new object[] { (___packageInteract as PackageInteractInputExtended).packageUseItem.IsPressed }, DispatchType.IMME, 2f);
            }
        }
        [HarmonyPatch(typeof(PackageUI_NotMain_Solution), "Update")]
        public static class PackageUI_NotMain_Solution_Update_Patch
        {
            public static void Postfix(PackageInteractInput ___packageInteract)
            {
                if (!enabled || Module<InputSolutionModule>.Self.CurSolutionType == SolutionType.ColorConfig)
                    return;
                MessageManager.Instance.Dispatch("UIOtherPackageSplitOne", new object[] { (___packageInteract as PackageInteractInputExtended).packageSplitOne.IsPressed }, DispatchType.IMME, 2f);
                MessageManager.Instance.Dispatch("UIOtherPackageSplitHalf", new object[] { (___packageInteract as PackageInteractInputExtended).packageSplitHalf.IsPressed }, DispatchType.IMME, 2f);
                MessageManager.Instance.Dispatch("UIOtherPackageUseItem", new object[] { (___packageInteract as PackageInteractInputExtended).packageUseItem.IsPressed }, DispatchType.IMME, 2f);
            }
        }
        [HarmonyPatch(typeof(PackageInteractInput), nameof(PackageInteractInput.CreateWithDefaultBindings))]
        public static class PackageInteractInput_CreateWithDefaultBindings_Patch
        {
            public static void Postfix(PackageInteractInput __instance, ref PackageInteractInput __result)
            {
                __result = new PackageInteractInputExtended();
            }
        }


        // CLICKING


        [HarmonyPatch(typeof(PackageExchangeUICtr), "StoreageClick")]
        public static class PackageExchangeUICtr_StoreageClick_Patch
        {
            public static bool Prefix(PackageExchangeUICtr __instance, ItemTable ___storeage)
            {
                if (!enabled)
                    return true;

                if (IsSplitHalf || IsSplitOne)
                {
                    var ___curSelectItem = (ItemObject)AccessTools.Property(typeof(PackageUIBase), "curSelectItem").GetValue(__instance, null);
                    if (___curSelectItem != null && ___curSelectItem.Number > 1)
                    {
                        int num = IsSplitHalf ? Mathf.CeilToInt(___curSelectItem.Number / 2f) : 1;
                        ItemObject itemObject = ItemObject.CreateItem(___curSelectItem.ItemDataId, num);
                        if (___storeage.AddItemObject(itemObject))
                        {
                            ___curSelectItem.ChangeNumber(-num);
                            __instance.FreshStoreage();
                            return false;
                        }
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(StoreageUIBase), "StoreageClick")]
        public static class StoreageUIBase_StoreageClick_Patch
        {
            public static bool Prefix(StoreageUIBase __instance)
            {
                if (!enabled)
                    return true;

                if (IsSplitHalf || IsSplitOne)
                {
                    var ___curSelectItem = (ItemObject)AccessTools.Property(typeof(PackageUIBase), "curSelectItem").GetValue(__instance, null);
                    var ___storeage = (ItemTable)AccessTools.Property(typeof(StoreageUIBase), "Storeage").GetValue(__instance, null);
                    if (___curSelectItem != null && ___curSelectItem.Number > 1)
                    {
                        int num = IsSplitHalf ? Mathf.CeilToInt(___curSelectItem.Number / 2f) : 1;
                        ItemObject itemObject = ItemObject.CreateItem(___curSelectItem.ItemDataId, num);
                        if (___storeage.AddItemObject(itemObject))
                        {
                            ___curSelectItem.ChangeNumber(-num);
                            __instance.FreshStoreage();
                            return false;
                        }
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(PackageUIBase), "BagClick")]
        public static class PackageUIBase_BagClick_Patch
        {
            public static bool Prefix(PackageUIBase __instance)
            {
                if (!enabled)
                    return true;

                if (IsSplitHalf || IsSplitOne)
                {
                    var ___curSelectItem = (ItemObject)AccessTools.Property(typeof(PackageUIBase), "curSelectItem").GetValue(__instance, null);
                    if (___curSelectItem != null && ___curSelectItem.Number > 1)
                    {
                        ItemObject tmpItem = ___curSelectItem;

                        int num = IsSplitHalf ? Mathf.CeilToInt(tmpItem.Number / 2f) : 1;
                        ItemObject itemObject = ItemObject.CreateItem(tmpItem.ItemDataId, num);
                        if (Module<Player>.Self.bag.AddItemFromSplit(itemObject, false))
                        {
                            tmpItem.ChangeNumber(-num);
                            __instance.FreshCurpageItem();
                            __instance.playerItemBar.FreshItem();
                            return false;
                        }
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(PackageUIBase), "ItemBarLeftClick")]
        public static class PackageUIBase_ItemBarLeftClick_Patch
        {
            public static bool Prefix(PackageUIBase __instance)
            {
                if (!enabled)
                    return true;

                if (IsSplitHalf || IsSplitOne)
                {
                    var ___curSelectItem = (ItemObject)AccessTools.Property(typeof(PackageUIBase), "curSelectItem").GetValue(__instance, null);
                    if (___curSelectItem != null && ___curSelectItem.Number > 1)
                    {
                        ItemObject tmpItem = ___curSelectItem;

                        int num = IsSplitHalf ? Mathf.CeilToInt(tmpItem.Number / 2f) : 1;
                        ItemObject itemObject = ItemObject.CreateItem(tmpItem.ItemDataId, num);
                        if (Module<Player>.Self.bag.AddItemFromSplit(itemObject, true))
                        {
                            tmpItem.ChangeNumber(-num);
                            __instance.FreshCurpageItem();
                            __instance.playerItemBar.FreshItem();
                            return false;
                        }
                    }
                }
                return true;
            }
        }


        // RIGHT CLICK


        [HarmonyPatch(typeof(StoreageUIBase), "RightClickStoreage")]
        public static class StoreageUIBase_RightClickStoreage_Patch
        {
            public static bool Prefix(StoreageUIBase __instance, int index)
            {
                if (!enabled)
                    return true;

                var itemObj = __instance.Storeage.GetItemObj(index);
                if (itemObj == null)
                    return true;
                if ((IsSplitHalf || IsSplitOne) && itemObj.Number > 1)
                {
                    int numTrans = IsSplitHalf ? Mathf.CeilToInt(itemObj.Number / 2f) : 1;
                    int numLeft = Module<Player>.Self.bag.GetItems(__instance.BagIndex).AddItemCount(itemObj.ItemDataId, numTrans);
                    numLeft = Module<Player>.Self.bag.itemBar.AddItemCount(itemObj.ItemDataId, numLeft);
                    itemObj.ChangeNumber(numLeft - numTrans);
                    if (numLeft > 0 && Module<Player>.Self.bag.GetFreeSlotCount(true) > 0)
                    {
                        ItemObject itemObjectNew = ItemObject.CreateItem(itemObj.ItemDataId, numLeft);
                        itemObj.ChangeNumber(-numLeft);
                        Module<Player>.Self.bag.AddItem(itemObjectNew, false, AddItemMode.Default);
                    }
                    __instance.FreshCurpageItem();
                    __instance.playerItemBar.FreshItem();
                    __instance.FreshStoreage();
                    __instance.storeageGrid.allIcons[index - __instance.storeageGrid.allIcons.Count * __instance.storeageGrid.curPage].SelectBg(true);
                    return false;
                }
                if (IsUseForced)
                {
                    __instance.storeageGrid.allIcons[index - __instance.storeageGrid.allIcons.Count * __instance.storeageGrid.curPage].SelectBg(true);
                    UseItem(__instance, itemObj);
                    __instance.FreshStoreageInfo(itemObj, index);
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(StoreageUIBase), "RightClickPackage")]
        public static class StoreageUIBase_RightClickPackage_Patch
        {
            public static bool Prefix(StoreageUIBase __instance, int index)
            {
                if (!enabled)
                    return true;

                var itemObj = Module<Player>.Self.bag.GetItem(__instance.BagIndex, index);
                if (itemObj == null)
                    return true;

                if ((IsSplitHalf || IsSplitOne) && itemObj.Number > 1)
                {
                    int numTrans = IsSplitHalf ? Mathf.CeilToInt(itemObj.Number / 2f) : 1;
                    int numLeft = __instance.Storeage.AddItemCount(itemObj.ItemDataId, numTrans);
                    itemObj.ChangeNumber(numLeft - numTrans);
                    if (numLeft > 0 && __instance.Storeage.GetVacancyCount() > 0)
                    {
                        ItemObject itemObjectNew = ItemObject.CreateItem(itemObj.ItemDataId, numLeft);
                        itemObj.ChangeNumber(-numLeft);
                        __instance.Storeage.AddItemObject(itemObjectNew);
                    }
                    __instance.FreshCurpageItem();
                    __instance.playerItemBar.FreshItem();
                    __instance.FreshStoreage();
                    __instance.packageGrid.allIcons[index - __instance.packageGrid.allIcons.Count * __instance.packageGrid.curPage].SelectBg(true);
                    return false;
                }
                if (IsUseForced)
                {
                    __instance.packageGrid.allIcons[index - __instance.packageGrid.allIcons.Count * __instance.packageGrid.curPage].SelectBg(true);
                    UseItem(__instance, itemObj);
                    __instance.FreshItemInfo(itemObj, index);
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(StoreageUIBase), "RightClickItemBar")]
        public static class StoreageUIBase_RightClickItemBar_Patch
        {
            public static bool Prefix(StoreageUIBase __instance, int index)
            {
                if (!enabled)
                    return true;

                var itemObj = Module<Player>.Self.bag.itemBar.itemBarItems[index];
                if (itemObj == null)
                    return true;

                if ((IsSplitHalf || IsSplitOne) && itemObj.Number > 1)
                {
                    int numTrans = IsSplitHalf ? Mathf.CeilToInt(itemObj.Number / 2f) : 1;
                    int numLeft = __instance.Storeage.AddItemCount(itemObj.ItemDataId, numTrans);
                    itemObj.ChangeNumber(numLeft - numTrans);
                    if (numLeft > 0 && __instance.Storeage.GetVacancyCount() > 0)
                    {
                        ItemObject itemObjectNew = ItemObject.CreateItem(itemObj.ItemDataId, numLeft);
                        itemObj.ChangeNumber(-numLeft);
                        __instance.Storeage.AddItemObject(itemObjectNew);
                    }
                    __instance.FreshCurpageItem();
                    __instance.playerItemBar.FreshItem();
                    __instance.FreshStoreage();
                    return false;
                }
                if (IsUseForced)
                {
                    __instance.playerItemBar.SelectBgDirect(index);
                    UseItem(__instance, itemObj);
                    __instance.playerItemBar.FreshItem();
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PackageExchangeUICtr), "RightClickStoreage")]
        public static class PackageExchangeUICtr_RightClickStoreage_Patch
        {
            public static bool Prefix(PackageExchangeUICtr __instance, int index)
            {
                if (!enabled)
                    return true;

                var itemObj = __instance.Storeage.GetItemObj(index);
                if (itemObj == null)
                    return true;
                if ((IsSplitHalf || IsSplitOne) && itemObj.Number > 1)
                {
                    int numTrans = IsSplitHalf ? Mathf.CeilToInt(itemObj.Number / 2f) : 1;
                    int numLeft = Module<Player>.Self.bag.GetItems(__instance.BagIndex).AddItemCount(itemObj.ItemDataId, numTrans);
                    numLeft = Module<Player>.Self.bag.itemBar.AddItemCount(itemObj.ItemDataId, numLeft);
                    itemObj.ChangeNumber(numLeft - numTrans);
                    if (numLeft > 0 && Module<Player>.Self.bag.GetFreeSlotCount(true) > 0)
                    {
                        ItemObject itemObjectNew = ItemObject.CreateItem(itemObj.ItemDataId, numLeft);
                        itemObj.ChangeNumber(-numLeft);
                        Module<Player>.Self.bag.AddItem(itemObjectNew, false, AddItemMode.Default);
                    }
                    __instance.FreshCurpageItem();
                    __instance.playerItemBar.FreshItem();
                    __instance.FreshStoreage();
                    __instance.storeageGrid.allIcons[index - __instance.storeageGrid.allIcons.Count * __instance.storeageGrid.curPage].SelectBg(true);
                    return false;
                }
                if (IsUseForced)
                {
                    __instance.storeageGrid.allIcons[index - __instance.storeageGrid.allIcons.Count * __instance.storeageGrid.curPage].SelectBg(true);
                    UseItem(__instance, itemObj);
                    __instance.FreshStoreageInfo(itemObj, index);
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PackageExchangeUICtr), "RightClickPackage")]
        public static class SPackageExchangeUICtr_RightClickPackage_Patch
        {
            public static bool Prefix(PackageExchangeUICtr __instance, int index)
            {
                if (!enabled)
                    return true;

                var itemObj = Module<Player>.Self.bag.GetItem(__instance.BagIndex, index);
                if (itemObj == null)
                    return true;

                if ((IsSplitHalf || IsSplitOne) && itemObj.Number > 1)
                {
                    int numTrans = IsSplitHalf ? Mathf.CeilToInt(itemObj.Number / 2f) : 1;
                    int numLeft = __instance.Storeage.AddItemCount(itemObj.ItemDataId, numTrans);
                    itemObj.ChangeNumber(numLeft - numTrans);
                    if (numLeft > 0 && __instance.Storeage.GetVacancyCount() > 0)
                    {
                        ItemObject itemObjectNew = ItemObject.CreateItem(itemObj.ItemDataId, numLeft);
                        itemObj.ChangeNumber(-numLeft);
                        __instance.Storeage.AddItemObject(itemObjectNew);
                    }
                    __instance.FreshCurpageItem();
                    __instance.playerItemBar.FreshItem();
                    __instance.FreshStoreage();
                    __instance.packageGrid.allIcons[index - __instance.packageGrid.allIcons.Count * __instance.packageGrid.curPage].SelectBg(true);
                    return false;
                }
                if (IsUseForced)
                {
                    __instance.packageGrid.allIcons[index - __instance.packageGrid.allIcons.Count * __instance.packageGrid.curPage].SelectBg(true);
                    UseItem(__instance, itemObj);
                    __instance.FreshItemInfo(itemObj, index);
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(PackageExchangeUICtr), "RightClickItemBar")]
        public static class PackageExchangeUICtr_RightClickItemBar_Patch
        {
            public static bool Prefix(StoreageUIBase __instance, int index)
            {
                if (!enabled)
                    return true;

                var itemObj = Module<Player>.Self.bag.itemBar.itemBarItems[index];
                if (itemObj == null)
                    return true;

                if ((IsSplitHalf || IsSplitOne) && itemObj.Number > 1)
                {
                    int numTrans = IsSplitHalf ? Mathf.CeilToInt(itemObj.Number / 2f) : 1;
                    int numLeft = __instance.Storeage.AddItemCount(itemObj.ItemDataId, numTrans);
                    itemObj.ChangeNumber(numLeft - numTrans);
                    if (numLeft > 0 && __instance.Storeage.GetVacancyCount() > 0)
                    {
                        ItemObject itemObjectNew = ItemObject.CreateItem(itemObj.ItemDataId, numLeft);
                        itemObj.ChangeNumber(-numLeft);
                        __instance.Storeage.AddItemObject(itemObjectNew);
                    }
                    __instance.FreshCurpageItem();
                    __instance.playerItemBar.FreshItem();
                    __instance.FreshStoreage();
                    return false;
                }
                if (IsUseForced)
                {
                    __instance.playerItemBar.SelectBgDirect(index);
                    UseItem(__instance, itemObj);
                    __instance.playerItemBar.FreshItem();
                    return false;
                }
                return true;
            }
        }


        // DRAGGING


        public static bool draggingPart = false;
        public static bool draggingOne = false;

        [HarmonyPatch(typeof(PlayerItemBarCtr), "DragBegin")]
        public static class PlayerItemBarCtr_DragBegin_Patch
        {
            public static bool Prefix(PlayerItemBarCtr __instance, int index, ref Image ___dragItem, ref int ___dragingIndex, List<PlayerItemBarSlot> ___slots)
            {
                var tmp = ___dragItem.GetComponentInChildren<TextMeshProUGUI>(true);
                tmp.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, tmp.outlineWidth);
                if (!enabled || (!IsSplitHalf && !IsSplitOne))
                    return true;

                try
                {

                    var IsDraging = AccessTools.Property(typeof(PlayerItemBarCtr), "IsDraging");
                    var item = Module<Player>.Self.bag.itemBar.itemBarItems[index];
                    if (item == null)
                    {
                        return false;
                    }
                    if (item.Number == 1)
                        return true;
                    if ((bool)IsDraging.GetValue(__instance, null))
                    {
                        AccessTools.Method(typeof(PlayerItemBarCtr), "DragEnd").Invoke(__instance, new object[] { ___dragingIndex });
                    }
                    int num = IsSplitHalf ? Mathf.CeilToInt(item.Number / 2f) : 1;
                    IsDraging.SetValue(__instance, true, null);
                    ___dragingIndex = index;
                    ___dragItem.gameObject.SetActive(true);
                    ___dragItem.sprite = ___slots[index].clickIcon.sprite;
                    tmp.text = num.ToString();
                    ___slots[index].num.text = (item.Number - num).ToString();
                    AccessTools.FieldRefAccess<PlayerItemBarSlot, int>(___slots[index], "curNumber") = item.Number - num;
                    draggingPart = true;
                    draggingOne = IsSplitOne;
                }
                catch (Exception)
                {
                    Debug.LogError("Drag Begin Failed");
                }
                if (__instance.OnDragBegin != null)
                {
                    __instance.OnDragBegin(index);
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(PlayerItemBarCtr), "DragEnd")]
        public static class PlayerItemBarCtr_DragEnd_Patch
        {
            public static bool Prefix(PlayerItemBarCtr __instance, int index, GridPage ___packagePage, ref Image ___dragItem, ref int ___dragingIndex, List<PlayerItemBarSlot> ___slots, Collider2D ___dragCol, PackageUIBase ___package)
            {
                if (!draggingPart)
                    return true;

                if (__instance.OnDragEnd != null)
                {
                    __instance.OnDragEnd(index);
                }
                ItemObject itemObjectSrc = Module<Player>.Self.bag.itemBar.itemBarItems[index];
                if (itemObjectSrc == null)
                {
                    return false;
                }
                bool flag = false;
                int num = -1;
                for (int i = 0; i < ___packagePage.allIcons.Count; i++)
                {
                    if (___dragCol.bounds.Intersects(___packagePage.allIcons[i].GetComponent<Collider2D>().bounds))
                    {
                        num = i + ___packagePage.curPage * ___packagePage.allIcons.Count;
                        flag = true;
                        break;
                    }
                }
                int splitNum = !draggingOne ? Mathf.CeilToInt(itemObjectSrc.Number / 2f) : 1;

                if (flag)
                {
                    ItemObject itemObjectDest = Module<Player>.Self.bag.GetItems(___package.BagIndex).GetItemObj(num);

                    ItemObject itemObject = TransferItems(itemObjectSrc, itemObjectDest);
                    if (itemObject != null)
                    {
                        Module<Player>.Self.bag.GetItems(___package.BagIndex).SetItemObj(num, itemObject);
                    }
                    EventSystem.current.SetSelectedGameObject(___package.packageGrid.allIcons[num - ___packagePage.curPage * ___packagePage.allIcons.Count].gameObject);
                    AccessTools.Method(typeof(PlayerItemBarCtr), "Unequip").Invoke(__instance, new object[0]);
                }
                else if (CheckItemBar(__instance, index, ___dragCol))
                {
                }
                else if (___package is PackageUICtr)
                {
                    PackageUICtr packageUICtr = ___package as PackageUICtr;
                    AccessTools.Method(typeof(PlayerItemBarCtr), "CheckEquip").Invoke(__instance, new object[] { Module<Player>.Self.bag.itemBar.itemBarItems[index], packageUICtr });
                    packageUICtr.playerInfo.FreshEquip();
                }
                else if (___package is CabinetUI)
                {
                    CabinetUI cabinetUI = ___package as CabinetUI;
                    cabinetUI.CheckItemBarToCabinet(index);
                }
                else if (___package is StoreageUIBase)
                {
                    StoreageUIBase storeageUIBase = ___package as StoreageUIBase;
                    for (int j = 0; j < storeageUIBase.storeageGrid.allIcons.Count; j++)
                    {
                        int num3 = j + storeageUIBase.storeageGrid.allIcons.Count * storeageUIBase.storeageGrid.curPage;
                        if (___dragCol.bounds.Intersects(storeageUIBase.storeageGrid.allIcons[j].GetComponent<Collider2D>().bounds))
                        {
                            if (num3 < storeageUIBase.storeageSize)
                            {
                                ItemObject itemObjectDest = storeageUIBase.Storeage.GetItemObj(num3);

                                ItemObject itemObject = TransferItems(itemObjectSrc, itemObjectDest);
                                if (itemObject != null)
                                {
                                    storeageUIBase.Storeage.SetItemObj(num3, itemObject);
                                }
                                storeageUIBase.storeageGrid.allIcons[j].SelectBg(true);
                            }
                            break;
                        }
                    }
                    storeageUIBase.FreshStoreage();
                }
                else if (___package is PackageExchangeUICtr)
                {
                    PackageExchangeUICtr packageExchangeUICtr = ___package as PackageExchangeUICtr;
                    for (int k = 0; k < packageExchangeUICtr.storeageGrid.allIcons.Count; k++)
                    {
                        int num5 = k + packageExchangeUICtr.storeageGrid.allIcons.Count * packageExchangeUICtr.storeageGrid.curPage;
                        if (___dragCol.bounds.Intersects(packageExchangeUICtr.storeageGrid.allIcons[k].GetComponent<Collider2D>().bounds))
                        {
                            if (num5 < packageExchangeUICtr.storeageSize)
                            {
                                ItemObject itemObjectDest = packageExchangeUICtr.Storeage.GetItemObj(num5);
                                ItemObject itemObject = TransferItems(itemObjectSrc, itemObjectDest);
                                if (itemObject != null)
                                {
                                    packageExchangeUICtr.Storeage.SetItemObj(num5, itemObject);
                                }

                                packageExchangeUICtr.storeageGrid.allIcons[k].SelectBg(true);
                            }
                            break;
                        }
                    }
                    packageExchangeUICtr.FreshStoreage();
                }
                __instance.FreshItem();
                AccessTools.Property(typeof(PlayerItemBarCtr), "IsDraging").SetValue(__instance, false, null);
                ___dragItem.gameObject.SetActive(false);
                draggingPart = false;
                return false;
            }

            private static bool CheckItemBar(PlayerItemBarCtr instance, int index, Collider2D dragCol)
            {
                List<Collider2D> allColliders = instance.GetAllColliders();
                for (int i = 0; i < allColliders.Count; i++)
                {
                    if (index != i)
                    {
                        if (dragCol.bounds.Intersects(allColliders[i].bounds))
                        {
                            ItemObject itemObjectSrc = Module<Player>.Self.bag.itemBar.itemBarItems[index];
                            ItemObject itemObjectDest = Module<Player>.Self.bag.itemBar.itemBarItems[i];
                            ItemObject itemObject = TransferItems(itemObjectSrc, itemObjectDest);
                            if (itemObject != null)
                            {
                                Module<Player>.Self.bag.itemBar.SetItemObject(itemObject, i);
                            }
                            instance.SelectBgDirect(i);
                            return true;
                        }
                    }
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(PackageUIBase), "DragBegin")]
        public static class PackageUIBase_DragBegin_Patch
        {
            public static bool Prefix(PackageUIBase __instance, int index, ref Image ___dragItem, ref int ___dragingIndex, bool ___isExchanging, ref bool ___isDraging)
            {
                var tmp = ___dragItem.GetComponentInChildren<TextMeshProUGUI>(true);
                tmp.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, tmp.outlineWidth);
                if (!enabled || (!IsSplitHalf && !IsSplitOne))
                    return true;

                AccessTools.Method(typeof(PackageUIBase), "DragingCheckBeforeBegin").Invoke(__instance, new object[0]);
                var item = Module<Player>.Self.bag.GetItem(__instance.BagIndex, index);

                if (index >= Module<Player>.Self.bag.GetTableSize(__instance.BagIndex) || item == null || ___isExchanging)
                {
                    return false;
                }
                if (item.Number == 1)
                    return true;
                try
                {
                    int num = IsSplitHalf ? Mathf.CeilToInt(item.Number / 2f) : 1;
                    ___isDraging = true;
                    ___dragingIndex = index;
                    ___dragItem.gameObject.SetActive(true);
                    ___dragItem.sprite = (__instance.packageGrid.allIcons[index - __instance.packageGrid.allIcons.Count * __instance.packageGrid.curPage] as GridIcon).clickIcon.sprite;
                    tmp.text = num.ToString();
                    (__instance.packageGrid.allIcons[index - __instance.packageGrid.allIcons.Count * __instance.packageGrid.curPage] as GridPackageIcon).num.text = (item.Number - num).ToString();
                    draggingPart = true;
                    draggingOne = IsSplitOne;
                }
                catch (Exception)
                {
                    global::UnityEngine.Debug.LogError("Drag Begin Failed");
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(PackageUIBase), "DragEnd")]
        public static class PackageUIBase_DragEnd_Patch
        {
            public static void Postfix(PackageUIBase __instance,  int index, Image ___dragItem, int ___dragingIndex, bool ___isExchanging, bool ___isDraging)
            {
                if (!draggingPart)
                    return;
                draggingPart = false;
                AccessTools.Method(typeof(PackageUIBase), "FreshItem").Invoke(__instance, new object[] { __instance.packageGrid.curPage });
                __instance.playerItemBar.FreshItem();
            }
        }
        [HarmonyPatch(typeof(PackageUIBase), "CheckSlotChange")]
        public static class PackageUIBase_CheckSlotChange_Patch
        {
            public static bool Prefix(PackageUIBase __instance, ref bool __result, int index, Image ___dragItem, int ___dragingIndex, bool ___isExchanging, bool ___isDraging, Collider2D ___dragItemCol)
            {
                if (!draggingPart)
                    return true;

                int num = -1;
                float num2 = -1f;
                for (int i = 0; i < __instance.packageGrid.allIcons.Count; i++)
                {
                    if (index - __instance.packageGrid.allIcons.Count * __instance.packageGrid.curPage != i)
                    {
                        if (___dragItemCol.bounds.Intersects(__instance.packageGrid.allIcons[i].GetComponent<Collider2D>().bounds))
                        {
                            float num3 = Vector3.Distance(___dragItemCol.transform.position, __instance.packageGrid.allIcons[i].transform.position);
                            if (num < 0 || num3 < num2)
                            {
                                num = i;
                                num2 = num3;
                            }
                        }
                    }
                }
                if (num < 0)
                {
                    __result = false;
                    return false;
                }
                int indexDest = num + __instance.packageGrid.curPage * __instance.packageGrid.allIcons.Count;
                ItemObject itemObjectSrc = Module<Player>.Self.bag.GetItems(__instance.BagIndex).GetItemObj(index);
                ItemObject itemObjectDest = Module<Player>.Self.bag.GetItems(__instance.BagIndex).GetItemObj(indexDest);
                ItemObject itemObject = TransferItems(itemObjectSrc, itemObjectDest);
                if (itemObject != null)
                {
                    Module<Player>.Self.bag.GetItems(__instance.BagIndex).SetItemObj(indexDest, itemObject);
                }
                EventSystem.current.SetSelectedGameObject(__instance.packageGrid.allIcons[num].gameObject);
                __result = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(PackageUIBase), "BagToItemBar")]
        public static class PackageUIBase_BagToItemBar_Patch
        {
            public static bool Prefix(PackageUIBase __instance, int slotIndex, int barIndex, int ___bagIndex, Image ___dragItem, int ___dragingIndex, bool ___isExchanging, bool ___isDraging)
            {
                if (!draggingPart)
                    return true;

                ItemObject itemObjectSrc = Module<Player>.Self.bag.GetItems(___bagIndex).GetItemObj(slotIndex);
                ItemObject itemObjectDest = Module<Player>.Self.bag.itemBar.itemBarItems[barIndex];
                ItemObject itemObject = TransferItems(itemObjectSrc, itemObjectDest);
                if (itemObject != null)
                {
                    Module<Player>.Self.bag.itemBar.SetItemObject(itemObject, barIndex);
                }
                __instance.playerItemBar.SelectBgDirect(barIndex);
                return false;
            }
        }

        [HarmonyPatch(typeof(StoreageUIBase), "DragBeginStoreage")]
        public static class StoreageUIBase_DragBeginStoreage_Patch
        {
            public static bool Prefix(StoreageUIBase __instance, int index, ref bool ___isDragingStorage, ref Image ___dragItem, ref int ___dragingIndex, bool ___isExchanging, ref bool ___isDraging)
            {
                var tmp = ___dragItem.GetComponentInChildren<TextMeshProUGUI>(true);
                tmp.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, tmp.outlineWidth);
                if (!enabled || (!IsSplitHalf && !IsSplitOne))
                    return true;

                AccessTools.Method(typeof(StoreageUIBase), "DragingCheckBeforeBegin").Invoke(__instance, new object[0]);

                var item = __instance.Storeage.GetItemObj(index);
                if (index >= __instance.storeageSize || item == null)
                {
                    return false;
                }
                int num = IsSplitHalf ? Mathf.CeilToInt(item.Number / 2f) : 1;

                ___isDragingStorage = true;
                ___dragingIndex = index;
                ___dragItem.gameObject.SetActive(true);
                ___dragItem.sprite = (__instance.storeageGrid.allIcons[index - __instance.storeageGrid.allIcons.Count * __instance.storeageGrid.curPage] as GridIcon).clickIcon.sprite;
                tmp.text = num.ToString();
                (__instance.storeageGrid.allIcons[index - __instance.storeageGrid.allIcons.Count * __instance.storeageGrid.curPage] as GridPackageIcon).num.text = (item.Number - num).ToString();
                draggingPart = true;
                draggingOne = IsSplitOne;
                return false;
            }
        }
        [HarmonyPatch(typeof(StoreageUIBase), "DragEnd")]
        public static class StoreageUIBase_DragEnd_Patch
        {
            public static void Postfix(StoreageUIBase __instance)
            {
                if (!draggingPart)
                    return;
                draggingPart = false;
                AccessTools.Method(typeof(StoreageUIBase), "FreshItem").Invoke(__instance, new object[] { __instance.packageGrid.curPage });
                AccessTools.Method(typeof(StoreageUIBase), "FreshStoreage", new Type[] { }).Invoke(__instance, new object[] { });
                __instance.playerItemBar.FreshItem();
            }
        }
        [HarmonyPatch(typeof(StoreageUIBase), "DragEndStoreage")]
        public static class StoreageUIBase_DragEndStoreage_Patch
        {
            public static void Postfix(StoreageUIBase __instance)
            {
                if (!draggingPart)
                    return;
                draggingPart = false;
                AccessTools.Method(typeof(StoreageUIBase), "FreshItem").Invoke(__instance, new object[] { __instance.packageGrid.curPage });
                AccessTools.Method(typeof(StoreageUIBase), "FreshStoreage", new Type[] { }).Invoke(__instance, new object[] { });
                __instance.playerItemBar.FreshItem();
            }
        }
        [HarmonyPatch(typeof(StoreageUIBase), "StorageToItemBar")]
        public static class StoreageUIBase_StorageToItemBar_Patch
        {
            public static bool Prefix(StoreageUIBase __instance, int fromIndex, int toIndex)
            {
                if (!draggingPart)
                    return true;

                ItemObject itemObjectSrc = __instance.Storeage.GetItemObj(fromIndex);
                ItemObject itemObjectDest = Module<Player>.Self.bag.itemBar.itemBarItems[toIndex];
                ItemObject itemObject = TransferItems(itemObjectSrc, itemObjectDest);
                if (itemObject != null)
                {
                    Module<Player>.Self.bag.itemBar.SetItemObject(itemObject, toIndex);
                }

                return false;
            }
        }
        [HarmonyPatch(typeof(StoreageUIBase), "PackageToPackage")]
        public static class StoreageUIBase_PackageToPackage_Patch
        {
            public static bool Prefix(StoreageUIBase __instance, int index, int curPackageIndex, int toIndex)
            {
                if (!draggingPart)
                    return true;


                ItemObject itemObjectSrc = Module<Player>.Self.bag.GetItems(__instance.BagIndex).GetItemObj(index);
                ItemObject itemObjectDest = Module<Player>.Self.bag.GetItems(__instance.BagIndex).GetItemObj(toIndex);
                ItemObject itemObject = TransferItems(itemObjectSrc, itemObjectDest);
                if (itemObject != null)
                {
                    Module<Player>.Self.bag.GetItems(__instance.BagIndex).SetItemObj(toIndex, itemObject);
                }

                return false;
            }
        }
        [HarmonyPatch(typeof(StoreageUIBase), "StorageToPackage")]
        public static class StoreageUIBase_StorageToPackage_Patch
        {
            public static bool Prefix(StoreageUIBase __instance, int index, int curPackageIndex, int toIndex)
            {
                if (!draggingPart)
                    return true;

                ItemObject itemObjectSrc = __instance.Storeage.GetItemObj(index);
                ItemObject itemObjectDest = Module<Player>.Self.bag.GetItems(__instance.BagIndex).GetItemObj(curPackageIndex);
                ItemObject itemObject = TransferItems(itemObjectSrc, itemObjectDest);
                if (itemObject != null)
                {
                    Module<Player>.Self.bag.GetItems(__instance.BagIndex).SetItemObj(curPackageIndex, itemObject);
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(StoreageUIBase), "PackageToStorage")]
        public static class StoreageUIBase_PackageToStorage_Patch
        {
            public static bool Prefix(StoreageUIBase __instance, int index, int curStorageIndex, int toIndex)
            {
                if (!draggingPart)
                    return true;

                ItemObject itemObjectSrc = Module<Player>.Self.bag.GetItems(__instance.BagIndex).GetItemObj(index);
                ItemObject itemObjectDest = __instance.Storeage.GetItemObj(curStorageIndex);
                ItemObject itemObject = TransferItems(itemObjectSrc, itemObjectDest);
                if (itemObject != null)
                {
                    __instance.Storeage.SetItemObj(curStorageIndex, itemObject);
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(StoreageUIBase), "StorageToStorage")]
        public static class StoreageUIBase_StorageToStorage_Patch
        {
            public static bool Prefix(StoreageUIBase __instance, int index, int curStorageIndex, int toIndex)
            {
                if (!draggingPart)
                    return true;

                ItemObject itemObjectSrc = __instance.Storeage.GetItemObj(index);
                ItemObject itemObjectDest = __instance.Storeage.GetItemObj(curStorageIndex);
                ItemObject itemObject = TransferItems(itemObjectSrc, itemObjectDest);
                if (itemObject != null)
                {
                    __instance.Storeage.SetItemObj(curStorageIndex, itemObject);
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(PackageExchangeUICtr), "DragBeginStoreage")]
        public static class PackageExchangeUICtr_DragBeginStoreage_Patch
        {
            public static bool Prefix(PackageExchangeUICtr __instance, int index, ref Image ___dragItem, ref int ___dragingIndex, bool ___isExchanging, ref bool ___isDraging)
            {
                var tmp = ___dragItem.GetComponentInChildren<TextMeshProUGUI>(true);
                tmp.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, tmp.outlineWidth);
                if (!enabled || (!IsSplitHalf && !IsSplitOne))
                    return true;

                var item = __instance.Storeage.GetItemObj(index);
                if (index >= __instance.storeageSize || item == null)
                {
                    return false;
                }
                if (___isDraging)
                {
                    AccessTools.Method(typeof(PackageExchangeUICtr), "DragEnd").Invoke(__instance, new object[] { ___dragingIndex });
                }

                int num = IsSplitHalf ? Mathf.CeilToInt(item.Number / 2f) : 1;
                ___isDraging = true;
                ___dragingIndex = index;
                ___dragItem.gameObject.SetActive(true);
                ___dragItem.sprite = (__instance.storeageGrid.allIcons[index - __instance.storeageGrid.allIcons.Count * __instance.storeageGrid.curPage] as GridIcon).clickIcon.sprite;
                tmp.text = num.ToString();
                (__instance.storeageGrid.allIcons[index - __instance.storeageGrid.allIcons.Count * __instance.storeageGrid.curPage] as GridPackageIcon).num.text = (item.Number - num).ToString();
                draggingPart = true;
                draggingOne = IsSplitOne;
                return false;
            }
        }

        [HarmonyPatch(typeof(PackageExchangeUICtr), "DragEnd")]
        public static class PackageExchangeUICtr_DragEnd_Patch
        {
            public static void Postfix(PackageExchangeUICtr __instance)
            {
                if (!draggingPart)
                    return;
                draggingPart = false;
                AccessTools.Method(typeof(PackageExchangeUICtr), "FreshItem").Invoke(__instance, new object[] { __instance.packageGrid.curPage });
                AccessTools.Method(typeof(PackageExchangeUICtr), "FreshStoreage", new Type[] { }).Invoke(__instance, new object[] { });
                __instance.playerItemBar.FreshItem();
            }
        }
        [HarmonyPatch(typeof(PackageExchangeUICtr), "DragEndStoreage")]
        public static class PackageExchangeUICtr_DragEndStoreage_Patch
        {
            public static void Postfix(PackageExchangeUICtr __instance)
            {
                if (!draggingPart)
                    return;
                draggingPart = false;
                AccessTools.Method(typeof(PackageExchangeUICtr), "FreshItem").Invoke(__instance, new object[] { __instance.packageGrid.curPage });
                AccessTools.Method(typeof(PackageExchangeUICtr), "FreshStoreage", new Type[] { }).Invoke(__instance, new object[] { });
                __instance.playerItemBar.FreshItem();
            }
        }
        [HarmonyPatch(typeof(PackageExchangeUICtr), "CheckItemBar", new Type[] { typeof(int), typeof(PackageArea) })]
        public static class PackageExchangeUICtr_CheckItemBar_Patch
        {
            public static bool Prefix(PackageExchangeUICtr __instance, ref bool __result, Collider2D ___dragItemCol, int index, PackageArea fromArea)
            {
                if (!draggingPart)
                    return true;

                List<Collider2D> allColliders = __instance.playerItemBar.GetAllColliders();
                for (int i = 0; i < allColliders.Count; i++)
                {
                    if (___dragItemCol.bounds.Intersects(allColliders[i].bounds))
                    {
                        if (fromArea == PackageArea.Bag)
                        {
                            ItemObject itemObjectSrc = Module<Player>.Self.bag.GetItems(__instance.BagIndex).GetItemObj(index);
                            ItemObject itemObjectDest = Module<Player>.Self.bag.itemBar.itemBarItems[i];
                            ItemObject itemObject = TransferItems(itemObjectSrc, itemObjectDest);
                            if (itemObject != null)
                            {
                                Module<Player>.Self.bag.itemBar.SetItemObject(itemObject, i);
                            }
                        }
                        else if (fromArea == PackageArea.Storage)
                        {
                            ItemObject itemObjectSrc = __instance.Storeage.GetItemObj(index);
                            ItemObject itemObjectDest = Module<Player>.Self.bag.itemBar.itemBarItems[i];
                            ItemObject itemObject = TransferItems(itemObjectSrc, itemObjectDest);
                            if (itemObject != null)
                            {
                                Module<Player>.Self.bag.itemBar.SetItemObject(itemObject, i);
                            }
                        }
                        __instance.playerItemBar.SelectBgDirect(i);
                        __result = true;
                    }
                }
                __result = false;
                return false;
            }
        }
        
        [HarmonyPatch(typeof(PackageExchangeUICtr), "CheckSlotChange", new Type[] { typeof(int), typeof(PackageArea) })]
        public static class PackageExchangeUICtr_CheckSlotChange_Patch
        {
            public static bool Prefix(PackageExchangeUICtr __instance, ref bool __result, Collider2D ___dragItemCol, int index, PackageArea fromArea)
            {
                if (!draggingPart)
                    return true;

                for (int i = 0; i < __instance.packageGrid.allIcons.Count; i++)
                {
                    int num = i + __instance.packageGrid.allIcons.Count * __instance.packageGrid.curPage;
                    if (___dragItemCol.bounds.Intersects(__instance.packageGrid.allIcons[i].GetComponent<Collider2D>().bounds))
                    {
                        if (fromArea == PackageArea.Bag)
                        {
                            if (index != num)
                            {
                                ItemObject itemObjectSrc = Module<Player>.Self.bag.GetItems(__instance.BagIndex).GetItemObj(index);
                                ItemObject itemObjectDest = Module<Player>.Self.bag.GetItems(__instance.BagIndex).GetItemObj(num);

                                ItemObject itemObject = TransferItems(itemObjectSrc, itemObjectDest);
                                if (itemObject != null)
                                {
                                    Module<Player>.Self.bag.GetItems(__instance.BagIndex).SetItemObj(num, itemObject);
                                }
                                __instance.packageGrid.allIcons[i].SelectBg(true);
                                __result = true;
                                return false;
                            }
                        }
                        else
                        {
                            if (fromArea == PackageArea.Storage && Module<Player>.Self.bag.GetItems(__instance.BagIndex).IsSlotUnlocked(num))
                            {
                                ItemObject itemObjectSrc = __instance.Storeage.GetItemObj(index);
                                ItemObject itemObjectDest = Module<Player>.Self.bag.GetItems(__instance.BagIndex).GetItemObj(num);

                                ItemObject itemObject = TransferItems(itemObjectSrc, itemObjectDest);
                                if (itemObject != null)
                                {
                                    Module<Player>.Self.bag.SetItem(__instance.BagIndex, num, itemObject);
                                }
                                __instance.packageGrid.allIcons[i].SelectBg(true);
                                __result = true;
                                return false;
                            }
                            break;
                        }
                    }
                }
                for (int j = 0; j < __instance.storeageGrid.allIcons.Count; j++)
                {
                    int num4 = j + __instance.storeageGrid.allIcons.Count * __instance.storeageGrid.curPage;
                    if (___dragItemCol.bounds.Intersects(__instance.storeageGrid.allIcons[j].GetComponent<Collider2D>().bounds))
                    {
                        if (fromArea == PackageArea.Storage)
                        {
                            if (index != num4)
                            {
                                ItemObject itemObjectSrc = __instance.Storeage.GetItemObj(index);
                                ItemObject itemObjectDest = __instance.Storeage.GetItemObj(num4); 
                                ItemObject itemObject = TransferItems(itemObjectSrc, itemObjectDest);
                                if (itemObject != null)
                                {
                                    __instance.Storeage.SetItemObj(num4, itemObject);
                                }
                                __result = true;
                                return false;
                            }
                        }
                        else
                        {
                            if (fromArea == PackageArea.Bag)
                            {
                                if (j < __instance.storeageSize)
                                {
                                    ItemObject itemObjectSrc = Module<Player>.Self.bag.GetItems(__instance.BagIndex).GetItemObj(index);
                                    ItemObject itemObjectDest = __instance.Storeage.GetItemObj(num4);
                                    ItemObject itemObject = TransferItems(itemObjectSrc, itemObjectDest);
                                    if (itemObject != null)
                                    {
                                        __instance.Storeage.SetItemObj(num4, itemObject);
                                    }
                                    __instance.storeageGrid.allIcons[j].SelectBg(true);
                                }
                                __result = true;
                                return false;
                            }
                            break;
                        }
                    }
                }
                __result = false;
                return false;
            }
        }
        public static ItemObject TransferItems(ItemObject itemObjectSrc, ItemObject itemObjectDest)
        {
            int splitNum = !draggingOne ? Mathf.CeilToInt(itemObjectSrc.Number / 2f) : 1;
            if (itemObjectDest != null && itemObjectSrc.ItemBase.ID == itemObjectDest.ItemBase.ID && itemObjectDest.RemainCapacity() != 0)
            {
                int transferNum = Math.Min(splitNum, itemObjectDest.RemainCapacity());
                itemObjectDest.ChangeNumber(transferNum);
                itemObjectSrc.ChangeNumber(-transferNum);

            }
            else if (itemObjectDest == null)
            {
                itemObjectSrc.ChangeNumber(-splitNum);
                return ItemObject.CreateItem(itemObjectSrc.ItemDataId, splitNum);
            }
            return null;
        }

        private static void MouseSplitHalf(object[] obj)
        {
            IsSplitHalf = (bool)obj[0];
        }

        private static void MouseSplitOne(object[] obj)
        {
            IsSplitOne = (bool)obj[0];
        }

        private static void MouseUseItem(object[] obj)
        {
            IsUseForced = (bool)obj[0];
        }

        public static void UseItem(PackageUIBase instance, ItemObject item)
        {
            if (item == null)
            {
                return;
            }
            Debug.Log("使用物品:\t" + item.ItemBase.Name);
            for (int i = 0; i < item.ItemBase.ItemType.Length; i++)
            {
                ItemType itemType = item.ItemBase.ItemType[i];
                switch (itemType)
                {
                    case ItemType.Equipment:
                        CheckEquipOnConfirm(item);
                        instance.FreshCurpageItem();
                        break;
                    default:
                        if (itemType != ItemType.BoxItem)
                        {
                            if (itemType != ItemType.CreationBook)
                            {
                                Debug.Log("背包使用");
                            }
                            else
                            {
                                Module<Player>.Self.UseCreationBook(item, true);
                            }
                        }
                        else
                        {
                            int gold;
                            ItemObject[] dropItems = Module<ItemDropManager>.Self.GetDropItemList(item.GetComponent<ItemBoxItemCmpt>().DropListID, out gold);
                            Module<Player>.Self.bag.RemoveItem(item, 1, true, true);

                            GameObject gameObject = GameUtils.AddChildToTransform(instance.transform, "Prefabs/ReceiveItemViewer", false, AssetType.UiSystem);
                            (gameObject.transform as RectTransform).sizeDelta = Vector2.zero;
                            ReceiveItemViewer viewer = gameObject.GetComponent<ReceiveItemViewer>();
                            AccessTools.FieldRefAccess<ReceiveItemViewer, Vector3>(viewer, "targetWorldPos") = viewer.transform.position;
                            viewer.ShowReceive(dropItems, gold);
                            viewer.EndShow += delegate
                            {
                                foreach (ItemObject itemObject in dropItems)
                                {
                                    Module<Player>.Self.bag.AddItem(itemObject, true, AddItemMode.Default);
                                }
                                Module<Player>.Self.bag.ChangeMoney(gold, true, 0);
                                global::UnityEngine.Object.Destroy(viewer.gameObject);
                            };
                        }
                        break;
                    case ItemType.Food:
                        Debug.Log("hp" + Module<Player>.Self.actor.intHp);
                        Debug.Log("cp" + Module<Player>.Self.actor.intCp);
                        if (Module<Player>.Self.actor == null)
                        {
                            Debug.Log("curActor = null");
                        }
                        else if (PackageManager.CheckLevel(item, false) && Module<Player>.Self.Use(item, true))
                        {
                            StartUseItemAnim(instance, item, "Eat", null, 0.45f);
                        }
                        break;
                    case ItemType.ProductionBook:
                        Debug.Log("技能书使用:" + item.ItemBase.ID);
                        if (Module<CompoundManager>.Self.UnLockBook(item.ItemBase.ID, true, true))
                        {
                            Module<Player>.Self.bag.RemoveItem(item, 1, false, true);
                            string text = item.ItemBase.Name + " : " + TextMgr.GetStr(201382, -1);
                            UIUtils.ShowTips(0, text, 1.5f, false, null);
                            Module<AudioModule>.Self.PlayEffect2D(33, false, true, false);
                        }
                        else
                        {
                            string text2 = item.ItemBase.Name + " : " + TextMgr.GetStr(100160, -1);
                            UIUtils.ShowTips(0, text2, 1.5f, false, null);
                        }
                        break;
                }
            }
            instance.FreshCurpageItem();
            instance.playerItemBar.FreshItem();
        }
        public static bool CheckEquipOnConfirm(ItemObject item)
        {
            ItemEquipmentCmpt component = item.GetComponent<ItemEquipmentCmpt>();
            if (component == null)
            {
                return false;
            }
            if (component.EquipType == ItemEquipType.Helmet || component.EquipType == ItemEquipType.Cloth || component.EquipType == ItemEquipType.Shoe || component.EquipType == ItemEquipType.Accessory)
            {
                EquipOn(item, PackageManager.GetEquipPos(item));
                return true;
            }
            return false;
        }
        public static bool EquipOn(ItemObject obj, ActorEquip.EEquipSlot slot)
        {
            if (!PackageManager.CheckActor(obj, true))
            {
                return false;
            }
            if (!PackageManager.CheckLevel(obj, true))
            {
                return false;
            }
            ItemObject itemObject = ItemObject.CreateItem(obj.ItemDataId, 0);
            if (Module<Player>.Self.bag.EquipSlot.SetTableSlot(slot, itemObject))
            {
                Module<Player>.Self.bag.RemoveItem(obj, 1, false, true);
                return true;
            }
            UIUtils.ShowTips(0, TextMgr.GetStr(101333, -1), 1f, false, null);
            return false;
        }
        public static bool lockItem;
        private static Coroutine curCoroutine;

        public static void StartUseItemAnim(PackageUIBase instance, ItemObject item, string animName, Action endCb = null, float intervalTime = -1f)
        {
            if (lockItem)
            {
                Module<Player>.Self.ShowHoldItem();
            }
            lockItem = true;
            Module<Player>.Self.HideHoldItem(false, true);
            if (curCoroutine != null)
            {
                instance.StopCoroutine(curCoroutine);
            }
            curCoroutine = instance.StartCoroutine(UseItemAnim(animName, endCb, item, intervalTime));
        }
        public static IEnumerator UseItemAnim(string animName, Action endCb, ItemObject item, float intervalTime)
        {
            yield return null;
            if (intervalTime > 0f)
            {
                yield return new WaitForSecondsRealtime(intervalTime);
                EndAnim(endCb);
            }
            else
            {
                yield return new WaitForSecondsRealtime(1.1f);
                EndAnim(endCb);
            }
            yield break;
        }
        public static void EndAnim(Action endCb)
        {
            if (lockItem)
            {
                lockItem = false;
                Module<Player>.Self.ShowHoldItem();
            }
            if (endCb != null)
            {
                endCb();
            }
        }
    }
}
