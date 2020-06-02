using Harmony12;
using Pathea;
using Pathea.InputSolutionNs;
using Pathea.ItemSystem;
using Pathea.MailNs;
using Pathea.ModuleNs;
using Pathea.StoreNs;
using Pathea.UISystemNs;
using Pathea.UISystemNs.Grid;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityModManagerNet;

namespace ShippingBin
{
    public partial class Main
    {

        [HarmonyPatch(typeof(GameResCenter), "UpdateDisplay")]
        static class GameResCenter_UpdateDisplay
        {
            static void Postfix(PlayerTargetMultiAction ___playerTarget)
            {
                if (!enabled)
                    return;

                ___playerTarget.SetAction(ActionType.ActionRoll, 101208, ActionTriggerMode.Normal);
            }
        }
        [HarmonyPatch(typeof(GameResCenter), "OnInteract")]
        static class GameResCenter_OnInteract
        {
            static void Postfix(ActionType type)
            {
                if (!enabled)
                    return;
                if (type == ActionType.ActionRoll)
                {
                    OpenShippingBin();
                }
            }
        }
        [HarmonyPatch(typeof(PackageExchangeUICtr), "FreshStoreage", new Type[] { typeof(GridPage), typeof(int) })]
        static class PackageExchangeUICtr_FreshStoreage
        {
            static void Postfix(PackageExchangeUICtr __instance, TextMeshProUGUI ___title)
            {
                if (!enabled)
                    return;
                Regex pattern = new Regex(@" \([0-9]{1,3}%\)");
                if (pattern.IsMatch(___title.text))
                {
                    int gols = getGols(__instance.GetStoreage());
                    ___title.text = string.Format("    <color=#FFE17E>{0}</color>", TextMgr.GetStr(101347, -1) + (gols > 0 ? " " + TextMgr.GetStr(101388, -1) +gols:"") + " (" + (Math.Round(Module<StoreManagerV40>.Self.CurPriceIndex * 100)) + "%)");
                }
            }

        }

        [HarmonyPatch(typeof(MailManager), "CheckHaveUnread")]
        static class MailManager_CheckHaveUnread
        {
            static void Postfix(List<MailItemBase> ___mailDic, ref bool __result)
            {
                if (!enabled || !settings.delayReceipt)
                    return;
                foreach (MailItemBase mailItemBase in ___mailDic)
                {
                    if (!mailItemBase.isRead && mailItemBase.sendDateTicks < Module<TimeManager>.Self.DateTime.Ticks)
                    {
                        __result = true;
                        return;
                    }
                }
                __result = false;
            }
        }

        [HarmonyPatch(typeof(MailManager), "GetUnreadNum")]
        static class MailManager_GetUnreadNum
        {
            static void Postfix(List<MailItemBase> ___mailDic, ref int __result)
            {
                if (!enabled || !settings.delayReceipt)
                    return;
                int num = 0;
                foreach (MailItemBase mailItemBase in ___mailDic)
                {
                    if (!mailItemBase.isRead && mailItemBase.sendDateTicks < Module<TimeManager>.Self.DateTime.Ticks)
                    {
                        num++;
                    }
                }
                __result = num;
            }
        }

        [HarmonyPatch(typeof(MailUICtr), "LoadMail")]
        static class MailUICtr_LoadMail
        {
            static void Postfix(ref List<MailItemBase> ___curList)
            {
                if (!enabled || !settings.delayReceipt)
                    return;
                List<MailItemBase> list = new List<MailItemBase>();
                foreach (MailItemBase mailItemBase in ___curList)
                {
                    if (mailItemBase.sendDateTicks < Module<TimeManager>.Self.DateTime.Ticks)
                    {
                        list.Add(mailItemBase);
                    }
                }
                ___curList = list;
            }
        }

        [HarmonyPatch(typeof(MailScroll), "FreshList")]
        static class MailScroll_FreshList
        {
            static void Prefix(ref List<MailItemBase> infos)
            {
                if (!enabled || !settings.delayReceipt)
                    return;
                List<MailItemBase> list = new List<MailItemBase>();
                foreach (MailItemBase mailItemBase in infos)
                {
                    if (mailItemBase.sendDateTicks < Module<TimeManager>.Self.DateTime.Ticks)
                    {
                        list.Add(mailItemBase);
                    }
                }
                infos = list;
            }
        }

        //[HarmonyPatch(typeof(MailPageReceive), "Show")]
        static class MailPageReceive_Show
        {
            static void Prefix(MailItemBase mail)
            {
            }
        }
        
    }
}
