using Harmony12;
using Pathea;
using Pathea.DungeonModuleNs;
using Pathea.ItemDropNs;
using Pathea.ItemSystem;
using Pathea.ModuleNs;
using Pathea.ScenarioNs;
using Pathea.SkillNs;
using Pathea.StoreNs;
using Pathea.TreasureRevealerNs;
using Pathea.UISystemNs;
using Pathea.UISystemNs.Grid;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;
using static Harmony12.AccessTools;

namespace CustomMerch
{
    public partial class Main
    {

       [HarmonyPatch(typeof(StoreProductData), "LoadDataBase")]
        static class StoreProductData_Patch
        {
            static void Postfix()
            {
                if (!enabled)
                    return;
                foreach(StoreItem item in storeItems)
                {
                    StoreProductData spdata = new StoreProductData()
                    {
                        productId = item.productId,
                        itemId = item.itemId,
                        currency_itemId = item.currency,
                        exchangeRateData = item.exchange,
                        reqMissionId = item.requireMissions,
                        isLimited = false,
                        dlcRequired = "",
                    };
                    Dbgl($"added item to database: {ItemDataMgr.Self.GetItemName(item.itemId)}");
                    typeof(StoreProductData).GetMethod("Init", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(spdata, new object[] { });
                    StoreProductData.refDataDic.Add(spdata);
                }
            }
        }

       [HarmonyPatch(typeof(StoreData), "Init")]
        static class StoreData_Init_Patch
        {
            static void Postfix(StoreData __instance, ref string[] ___generalProduct, ref SaleProductData[] ___generalProductData)
            {
                if (!enabled)
                    return;
                if (___generalProduct == null)
                {
                    ___generalProduct = new string[0];
                    ___generalProductData = new SaleProductData[0];
                }
                int i = ___generalProduct.Length;

                string[] newProduct = new string[i + storeItems.Count];
                SaleProductData[] newProductData = new SaleProductData[i + storeItems.Count];

                Array.Copy(___generalProduct, newProduct, i);
                Array.Copy(___generalProductData, newProductData, i);

                foreach(StoreItem item in storeItems)
                { 
                    if(item.storeId == __instance.id)
                    {
                        string data = $"{item.productId}_{item.count}{(item.chance < 1f ? "_" + item.chance : "")}";
                        newProduct[i] = data;
                        newProductData[i++] = new SaleProductData(data);
                        Dbgl($"added item to store: {ItemDataMgr.Self.GetItemName(item.itemId)} {new SaleProductData(data).productId}");
                    }
                }
                ___generalProduct = newProduct;
                ___generalProductData = newProductData;
            }
        }

        //[HarmonyPatch(typeof(TextMgr), "Get")]
        private static class TextMgr_Get_Patch
        {
            private static bool Prefix(TextMgr __instance, int id, ref string __result)
            {
                if (!enabled)
                    return true;

                if (nameDesc != null && nameDesc.ContainsKey(id))
                {
                    __result = nameDesc[id];
                    return false;
                }
                return true;
            }
        }
        
        //[HarmonyPatch(typeof(ItemDataMgr), "OnLoad")]
        static class ItemDataMgr_OnLoad_Patch
        {
            static void Postfix(ItemDataMgr __instance, ref List<ItemBaseConfData> ___itemBaseList, ref List<EquipmentItemConfData> ___equipmentDataList)
            {
                if (!enabled)
                    return;
                int itemIds = 42000000;
                int nameIds = 42000000;
                int sourceIds = 4200;

                foreach (NewItem item in newItems)
                {
                    nameDesc.Add(nameIds, item.name);
                    nameDesc.Add(nameIds+1, item.description);
                    nameDesc.Add(nameIds+2, item.effect);
                    nameDesc.Add(nameIds+3, storeNames[item.storeId]);
                    sourceStrings.Add(sourceIds, nameIds + 3);

                    ItemBaseConfData itemConf = new ItemBaseConfData()
                    {
                        ID = itemIds,
                        NameID = nameIds,
                        Explain_one = nameIds+1,
                        BuyPrice = item.buyPrice,
                        SellPrice = item.sellPrice,
                        ExhibitModelPath = item.dropModelPath,
                        giftModelPath = item.dropModelPath,
                        displayScale = item.displayScale,
                        ItemType = new ItemType[] { ItemType.Equipment },

                        sourceIndex = new int[] { sourceIds },
                        orderIndex = item.orderIndex,

                        ItemFunctionID = itemIds,
                        StackLimitedNumber = 1,
                        isGift = 0,
                        rareLv = 0,
                        reputationModify = 0,

                        intendType = item.intendType,
                        skillIds = item.skillIds,
                        alwaysOnHand = item.alwaysOnHand,
                        holdInBothHands = item.holdInBothHands,
                        energy = -1,
                        hotValue = 0,
                        recycleTag = "1",
                        Effect_Desc_ID = nameIds+2,
                        HaveIcon = true,
                        CanPickup = true,
                        CanDiscard = true,
                        CanSell = true,
                    };

                    typeof(ItemBaseConfData).GetField("iconPath", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(itemConf, item.iconPath);
                    typeof(ItemBaseConfData).GetField("ModelPath", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(itemConf, item.modelPath);
                    typeof(ItemBaseConfData).GetField("DropModelPathMale", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(itemConf, item.dropModelPath);

                    ___itemBaseList.Add(itemConf);

                    EquipmentItemConfData equipConf = new EquipmentItemConfData()
                    {
                        id = itemIds,
                        equipType = 0,
                        durabilityMax = 100,
                        durabilityCost = 0,
                        repairRate = 1,
                        attack = item.attack,
                        defense = item.defense,
                        critical = item.critical,
                        antiCritical = item.antiCritical,
                        hpMax = item.hpMax,
                        cpMax = item.cpMax,
                        attackType = AttackType.Melee,
                        digGridCount = item.digGridCount,
                        digIntensity = item.digIntensity,
                        Rate = item.rate,
                        dateForce = 0,
                        meleeCriticalAmount = item.meleeCriticalAmount,
                        rangeCriticalAmount = 0,
                    };

                    ___equipmentDataList.Add(equipConf);

                    storeItems.Add(new StoreItem(itemIds, productIds++, 1, -1, "-1", item.storeId, item.chance));

                    itemIds++;
                    nameIds += 4;
                    sourceIds++;

                }
            }
        }

        //[HarmonyPatch(typeof(ItemSourceDB), "GetDesc")]
        private static class ItemSourceDB_GetDesc_Patch
        {
            private static bool Prefix(int index, ref int __result)
            {
                if (!enabled)
                    return true;
                Dbgl("get desc "+index);
                if (sourceStrings.ContainsKey(index))
                {
                    Dbgl("get source for " + index);
                    __result = sourceStrings[index];
                    return false;
                }
                return true;
            }
        }
    }
}