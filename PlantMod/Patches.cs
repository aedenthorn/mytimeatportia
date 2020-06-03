using Harmony12;
using Hont;
using Pathea;
using Pathea.HomeNs;
using Pathea.ItemSystem;
using Pathea.ModuleNs;
using Pathea.StoreNs;
using Pathea.UISystemNs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityModManagerNet;

namespace PlantMod
{
    public static partial class Main
    {

        [HarmonyPatch(typeof(NutrientContainerUnit), "ChangeNutrient")]
        static class NutrientContainerUnit_ChangeNutrient_Patch
        {
            static void Prefix(NutrientContainerUnit __instance, ref float value)
            {
                if (!enabled || __instance.GetType() != typeof(PlantingBoxUnit) || value > 0)
                    return;

                value *= settings.nutrientConsumeMult;

            }
        }
        [HarmonyPatch(typeof(Plant), "CalculateGrowSeconds")]
        static class PlantingBoxUnit_Grow_Patch
        {
            static void Prefix(ref float second)
            {
                if (!enabled)
                    return;

                second *= settings.plantGrowMult;
            }
        }

        [HarmonyPatch(typeof(PlantingBoxInfoUI), "FreshGrowDisplay")]
        static class PlantingBoxInfoUI_FreshGrowDisplay_Patch
        {
            static void Postfix(PlantingBoxInfoUI __instance, bool isInit, ref TextMeshProUGUI ___progText, bool ___isEnable, bool ___riped, bool ___isBadSeason, GameTimeSpan ___timeToRipe)
            {
                if (!enabled)
                    return;
                if (!isInit)
                {
                    if (!__instance.enabled)
                    {
                        return;
                    }
                    if (!___isEnable)
                    {
                        return;
                    }
                }

                GameTimeSpan gts = new GameTimeSpan((long)Math.Round(___timeToRipe.Ticks/settings.plantGrowMult));

                if (!___riped && !___isBadSeason && gts.TotalDays <= 99.0)
                {
                    if (gts.TotalDays < 1)
                    {
                        ___progText.text = string.Format(TextMgr.GetStr(100373, -1), gts.Hours, gts.Minutes);
                    }
                    else if (gts.TotalDays < 2)
                    {
                        ___progText.text = string.Format($"{TextMgr.GetStr(100972, -1)} {TextMgr.GetStr(100373, -1)}", (int)gts.TotalDays, gts.Hours, gts.Minutes).Replace("(s)","").Replace("(e)","");
                    }
                    else
                    {
                        ___progText.text = string.Format($"{TextMgr.GetStr(100972, -1)} {TextMgr.GetStr(100373, -1)}", (int)gts.TotalDays, gts.Hours, gts.Minutes).Replace("(s)","s").Replace("(e)", "e");
                    }
                }
            }
        }
        [HarmonyPatch(typeof(UIUtils), "GetSpriteByPath")]
        static class UIUtils_GetSpriteByPath
        {
            static void Postfix(ref Sprite __result, string path)
            {
                if (!enabled)
                    return;
                if (path.EndsWith("Item_seed_lemon"))
                {
                    string file = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\assets\\Item_seed_lemon.png";
                    Texture2D tex = new Texture2D(2, 2);
                    byte[] imageData = File.ReadAllBytes(file);
                    tex.LoadImage(imageData);
                    Sprite sprite = Sprite.Create(tex, __result.rect, __result.pivot, __result.pixelsPerUnit);
                    __result = sprite;
                }
            }

        }
        [HarmonyPatch(typeof(ItemDataMgr), "OnLoad")]
        static class ItemDataMgr_OnLoad
        {
            static void Postfix(ref List<ItemBaseConfData> ___itemBaseList)
            {
                if (!enabled)
                    return;

                int idx = ___itemBaseList.FindIndex((ItemBaseConfData e) => e.ID == 6000019);
                ___itemBaseList[idx].NameID = 270368;
                ___itemBaseList[idx].typeDesc = 100339;
                ___itemBaseList[idx].Effect_Desc_ID = 280039;
                AccessTools.FieldRefAccess<ItemBaseConfData, string>(___itemBaseList[idx], "iconPath") = "Sprites/Package/Item_seed_lemon";

                if (settings.ignoreSeasons)
                {
                    SetIgnoreSeasons(true);
                }
            }

        }

        [HarmonyPatch(typeof(StoreProductData), "LoadDataBase")]
        static class StoreProductData_Patch
        {
            static void Postfix()
            {
                if (!enabled)
                    return;
                foreach (StoreItem item in storeItems)
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
                    Dbgl($"added item to store database: {ItemDataMgr.Self.GetItemName(item.itemId)}");
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
                if (___generalProduct == null || __instance.id != 1) 
                {
                    return;
                }
                int i = ___generalProduct.Length;

                string[] newProduct = new string[i + storeItems.Count];
                SaleProductData[] newProductData = new SaleProductData[i + storeItems.Count];

                Array.Copy(___generalProduct, newProduct, i);
                Array.Copy(___generalProductData, newProductData, i);

                foreach (StoreItem item in storeItems)
                {
                    string data = $"{item.productId}_{item.count}{(item.chance < 1f ? "_" + item.chance : "")}";
                    newProduct[i] = data;
                    newProductData[i++] = new SaleProductData(data);
                    Dbgl($"added item to store: {ItemDataMgr.Self.GetItemName(item.itemId)} {new SaleProductData(data).productId}");
                }
                ___generalProduct = newProduct;
                ___generalProductData = newProductData;
            }
        }
    }
}
