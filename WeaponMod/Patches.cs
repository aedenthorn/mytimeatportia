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

namespace WeaponMod
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
                foreach(Weapon weapon in weapons)
                {
                    StoreProductData spdata = new StoreProductData()
                    {
                        productId = weapon.productId,
                        itemId = weapon.itemId,
                        currency_itemId = weapon.currency,
                        exchangeRateData = weapon.exchange,
                        reqMissionId = weapon.requireMissions,
                        isLimited = false,
                        dlcRequired = "",
                    };
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
                Dbgl(__instance.storeIcon);
                if (___generalProduct == null)
                {
                    return;
                }
                int i = ___generalProduct.Length;

                string[] newProduct = new string[i + weapons.Count];
                SaleProductData[] newProductData = new SaleProductData[i + weapons.Count];

                Array.Copy(___generalProduct, newProduct, i);
                Array.Copy(___generalProductData, newProductData, i);

                foreach(Weapon weapon in weapons)
                { 
                    if(weapon.storeId == __instance.id)
                    {
                        string data = $"{weapon.productId}_{weapon.count}{(weapon.chance < 1f ? "_" + weapon.chance : "")}";
                        newProduct[i] = data;
                        newProductData[i++] = new SaleProductData(data);
                    }
                }
                ___generalProduct = newProduct;
                ___generalProductData = newProductData;
            }
        }

        [HarmonyPatch(typeof(TextMgr), "Get")]
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
        
        [HarmonyPatch(typeof(ItemDataMgr), "OnLoad")]
        static class ItemDataMgr_OnLoad_Patch
        {
            static void Postfix(ItemDataMgr __instance, ref List<ItemBaseConfData> ___itemBaseList, ref List<EquipmentItemConfData> ___equipmentDataList)
            {
                if (!enabled)
                    return;
                int itemIds = 42000000;
                int nameIds = 42000000;
                int sourceIds = 4200;
                if (settings.includeSpecialWeapons)
                {
                    int storeId = storeNames.Keys.ToArray()[settings.specialWeaponStore];

                    weapons = new List<Weapon> {
                        new Weapon(1001999,productIds++,1,-1,"-1", storeId, 0.1f), // nameless
                        new Weapon(1001011,productIds++,1,-1,"-1", storeId, 0.5f), // Waterfall
                        //new Weapon(1001010,productIds++,1,-1,"-1", storeId, 0.5f), //Rogue Knight
                        new Weapon(1001012,productIds++,1,-1,"-1", storeId, 0.5f), // Purple Haze
                        new Weapon(1001402,productIds++,1,-1,"-1", storeId, 0.5f), // Inflateable Hammer
                        new Weapon(1001000,productIds++,1,-1,"-1", storeId, 0.5f), // Dev's Dagger
                    };
                }

                for (int i = 0; i < ___itemBaseList.Count; i++)
                {

                    if (___itemBaseList[i].ID == 1001999)
                    {
                        ___itemBaseList[i].BuyPrice = 1000000;
                    }
                }


                foreach (WeaponItem item in weaponItems)
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

                    weapons.Add(new Weapon(itemIds, productIds++, 1, -1, "-1", item.storeId, item.chance));

                    itemIds++;
                    nameIds += 4;
                    sourceIds++;

                }
            }
        }


        [HarmonyPatch(typeof(Pathea.SkillNs.Factory), "Load")]
        private static class SkillNs_Factory_Load_Patch
        {
            private static void Postfix(ref Dictionary<int, JSONNode> ___mDic)
            {
                if (!enabled)
                    return;
                string path = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\assets\\skills.json";
                try
                {
                    JSONNode jsonnode = JSONNode.Parse(File.ReadAllText(path));
                    if (!(jsonnode == null))
                    {
                        for (int i = 0; i < jsonnode.Count; i++)
                        {
                            JSONNode jsonnode2 = jsonnode[i];
                            int asInt = jsonnode2["id"].AsInt;
                            if (!___mDic.ContainsKey(asInt))
                            {
                                ___mDic.Add(asInt, jsonnode2);
                            }
                            else
                            {
                                Debug.LogWarning("duplicated skill:" + asInt);
                            }
                        }
                    }
                }
                catch (Exception arg)
                {
                    Debug.LogError("handled exception:" + arg);
                }
            }
        }

        [HarmonyPatch(typeof(ItemSourceDB), "GetDesc")]
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