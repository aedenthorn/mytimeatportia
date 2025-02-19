using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Harmony12;
using Pathea;
using Pathea.FarmFactoryNs;
using Pathea.HomeNs;
using Pathea.HomeViewerNs;
using Pathea.InputSolutionNs;
using Pathea.ItemBoxNs;
using Pathea.ItemSystem;
using Pathea.MessageSystem;
using Pathea.ModuleNs;
using Pathea.SpawnNs;
using Pathea.UISystemNs;
using Pathea.UISystemNs.UIBase;
using UnityEngine;
using UnityModManagerNet;
using static Harmony12.AccessTools;

namespace QuickStore
{
    public class Main
    {
        private static readonly bool isDebug = false;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log($"{(pref ? typeof(Main).Namespace : "") } {str}");
        }
        public static Settings settings { get; private set; }
        public static bool enabled;
        public static UnityModManager.ModEntry context;
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
            GUILayout.Label("<b>Store Key:</b>", new GUILayoutOption[0]);
            settings.StoreKey = GUILayout.TextField(settings.StoreKey, new GUILayoutOption[0]);
            GUILayout.Space(10);
            GUILayout.Label(string.Format("Factory Storage Size: <b>{0}</b>", settings.FactoryStorageSize), new GUILayoutOption[0]);
            settings.FactoryStorageSize = (int)GUILayout.HorizontalSlider((float)settings.FactoryStorageSize / 30, 10, 100f) * 30;
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

        private static bool isSorting;

        [HarmonyPatch(typeof(PlayerItemBarCtr), "Update")]
        static class ItemBar_Patch
        {

            static void Prefix(PlayerItemBarCtr __instance)
            {
                if (!enabled)
                    return;
                if (KeyDown(settings.StoreKey) && UIStateMgr.Instance.currentState.type == UIStateMgr.StateType.Play)
                {
                    isSorting = true;
                    StorageUnit.SortBagToStorageAsync();
                    SortBagToFactory();
                    isSorting = false;
                }
            }
        }

        // Token: 0x060034A7 RID: 13479 RVA: 0x00149E30 File Offset: 0x00148030
        public static void SortBagToFactory()
        {
            FarmFactory[] factorys = Module<FarmFactoryMgr>.Self.GetAllFactorys();

            if (factorys.Length == 0)
                return;
            for (var f = 0; f < factorys.Length; f++)
            {

                for (int j = factorys[f].MatList.Count - 1; j >= 0; j--)
                {
                    int id = factorys[f].MatList[j].id;
                    int itemCount = Module<Player>.Self.bag.GetItemCount(id);
                    if (itemCount <= 0)
                    {
                        continue;
                    }
                    ItemBaseConfData data = ItemDataMgr.Instance.GetItemBaseData(id);
                    int add = Math.Min(itemCount, data.StackLimitedNumber - factorys[f].MatList[j].count);
                    if (add > 0)
                    {
                        factorys[f].MatList[j].count += add;
                        Module<Player>.Self.bag.RemoveItem(id, add);
                    }
                    itemCount = Module<Player>.Self.bag.GetItemCount(id);
                    while (factorys[f].MatList.Count < settings.FactoryStorageSize && itemCount > 0)
                    {
                        add = Math.Min(itemCount, data.StackLimitedNumber);
                        factorys[f].MatList.Add(new IdCount(id, add));
                        Module<Player>.Self.bag.RemoveItem(id, add);
                        itemCount = Module<Player>.Self.bag.GetItemCount(id);
                    }
                }
            }
        }
        [HarmonyPatch(typeof(ItemBag), nameof(ItemBag.RemoveItem), new Type[] { typeof(int), typeof(int), typeof(bool), typeof(bool) })]
        static class ItemBag_RemoveItem_Patch
        {

            static void Prefix(ref bool showTips, int count)
            {
                if (!enabled || !isSorting)
                    return;
                showTips = count != 0;
            }
        }


        [HarmonyPatch(typeof(StoreageUICtr), nameof(StoreageUICtr.SortBagToStorageCheck))]
        static class StoreageUICtr_SortBagToStorageCheck_Patch
        {
            static bool Prefix(StoreageUICtr __instance)
            {
                if (!enabled || !settings.SkipConfirm)
                    return true;
                __instance.SortBagToStorage();
                return false;

            }
        }
    }
}
