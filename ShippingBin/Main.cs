using Harmony12;
using Pathea;
using Pathea.InputSolutionNs;
using Pathea.ItemSystem;
using Pathea.MailNs;
using Pathea.MessageSystem;
using Pathea.ModuleNs;
using Pathea.ScenarioNs;
using Pathea.StoreNs;
using Pathea.UISystemNs;
using Pathea.UISystemNs.Grid;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;

namespace ShippingBin
{
    public partial class Main
    {
        private static bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? typeof(Main).Namespace + "  " : "") + str);
        }

        public static bool enabled;
        public static Settings settings { get; private set; }

        // Send a response to the mod manager about the launch status, success or not.
        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Module<TimeManager>.Self.HourChangedEvent += HourChange;

            return true;
        }

        private static void HourChange()
        {
            MessageManager.Instance.Dispatch("OnReceivedMail", null, DispatchType.IMME, 2f);
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            settings.delayReceipt = GUILayout.Toggle(settings.delayReceipt, "Receive gols the next day morning (turn this off to receive gols instantly)", new GUILayoutOption[0]);
        }
        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }

        private static void OpenShippingBin()
        {
            if (!enabled)
                return;

            Action<List<IdCount>> action = delegate (List<IdCount> ls)
            {
                SellItems(ls);
            };
            UIStateMgr.Instance.ChangeStateByType(UIStateMgr.StateType.PackageExchangeState, true, new object[]
            {
                        new List<IdCount>(),
                        TextMgr.GetStr(101347, -1) + " (" + (Math.Round(Module<StoreManagerV40>.Self.CurPriceIndex*100))+"%)",
                        false,
                        action,
                        103521,
                        300
            });
        }

        private static int getGols(List<IdCount> items)
        {
            int gols = 0;
            foreach (IdCount idc in items)
            {
                int g = (int)Math.Round(Module<ItemDataMgr>.Self.GetItemSellPrice(idc.id) * idc.count * Module<StoreManagerV40>.Self.CurPriceIndex);
                gols += g;
            }
            return gols;
        }


        private static void SellItems(List<IdCount> ls)
        {
            int gols = getGols(ls);
            MailShipping ms = new MailShipping(gols);
            Module<MailManager>.Self.SendToMailBox(ms);
        }
    }
}
