using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony12;
using Pathea;
using Pathea.FarmFactoryNs;
using Pathea.HomeNs;
using Pathea.HomeViewerNs;
using Pathea.InputSolutionNs;
using Pathea.ItemSystem;
using Pathea.MessageSystem;
using Pathea.ModuleNs;
using Pathea.SpawnNs;
using Pathea.UISystemNs;
using UnityEngine;
using UnityModManagerNet;
using static Harmony12.AccessTools;

namespace StorageAnywhere
{
    public class Main
    {
        private static readonly bool isDebug = false;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "StorageAnywhere " : "") + str);
        }
        public static Settings settings { get; private set; }
        public static bool enabled;
        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnToggle = OnToggle;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            return;
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
            GUILayout.Label(string.Format("Itembar Switch Key:"), new GUILayoutOption[0]);
            settings.ItemBarSwitchKey = GUILayout.TextField(settings.ItemBarSwitchKey, new GUILayoutOption[0]);
            GUILayout.Space(20);
            GUILayout.Label(string.Format("Open Storage Key:"), new GUILayoutOption[0]);
            settings.OpenStorageKey = GUILayout.TextField(settings.OpenStorageKey, new GUILayoutOption[0]);
            GUILayout.Space(20);
            GUILayout.Label(string.Format("Open Factory Storage Key:"), new GUILayoutOption[0]);
            settings.OpenFactoryKey = GUILayout.TextField(settings.OpenFactoryKey, new GUILayoutOption[0]);
            GUILayout.Space(20);
            GUILayout.Label(string.Format("Switch to Prev Storage Key:"), new GUILayoutOption[0]);
            settings.PrevStorageKey = GUILayout.TextField(settings.PrevStorageKey, new GUILayoutOption[0]);
            GUILayout.Space(20);
            GUILayout.Label(string.Format("Switch to Next Storage Key:"), new GUILayoutOption[0]);
            settings.NextStorageKey = GUILayout.TextField(settings.NextStorageKey, new GUILayoutOption[0]);

        }


        [HarmonyPatch(typeof(PlayerItemBarCtr), "Update")]
        static class ItemBar_Patch
        {
            static void Prefix(PlayerItemBarCtr __instance)
            {
                if (!enabled)
                    return;

                if (Input.GetKeyDown(settings.ItemBarSwitchKey))
                {
                    for (int index = 0; index < 8; index++)
                    {
                        ItemObject itemObject = Module<Player>.Self.bag.itemBar.itemBarItems[index];
                        ItemObject itemObj = Module<Player>.Self.bag.GetItems(0).GetItemObj(index);
                        Module<Player>.Self.bag.BagExchangeItemBar(index, index, 0);
                    }

                    MethodInfo dynMethod = __instance.GetType().GetMethod("Unequip", BindingFlags.NonPublic | BindingFlags.Instance);
                    dynMethod.Invoke(__instance, new object[] { });
                }
                else if ( Input.GetKeyDown(settings.OpenStorageKey) && UIStateMgr.Instance.currentState.type == UIStateMgr.StateType.Play)
                {
                    StorageViewer sv = new StorageViewer();
                    FieldRef<StorageViewer, StorageUnit> suRef = FieldRefAccess<StorageViewer, StorageUnit>("storageUnit");
                    suRef(sv) = StorageUnit.GetStorageByGlobalIndex(lastStorageIndex);

                    MethodInfo dynMethod = sv.GetType().GetMethod("InteractStorage", BindingFlags.NonPublic | BindingFlags.Instance);
                    dynMethod.Invoke(sv, new object[] { });
                }
                else if ( Input.GetKeyDown(settings.OpenFactoryKey) && UIStateMgr.Instance.currentState.type == UIStateMgr.StateType.Play)
                {

                    FarmFactory[] factorys = Module<FarmFactoryMgr>.Self.GetAllFactorys();

                    if (factorys.Length == 0)
                        return;
                    FarmFactory factory = factorys[0];

                    Action<List<IdCount>> action = delegate (List<IdCount> ls)
                    {
                        factory.SetMatList(ls);
                    };
                    UIStateMgr.Instance.ChangeStateByType(UIStateMgr.StateType.PackageExchangeState, true, new object[]
                    {
                        factory.MatList,
                        TextMgr.GetStr(103440, -1),
                        true,
                        action,
                        103521,
                        300
                    });

                }
            }
        }

        private static int lastStorageIndex = 0;

        [HarmonyPatch(typeof(StoreageUICtr), "SetStorageByGlobalIndex", new Type[]{typeof(int)})]
        static class SetStorageByGlobalIndex_Patch
        {
            static void Postfix(int index)
            {
                if (!enabled || !settings.RememberLastStorageUnit)
                    return;

                lastStorageIndex = index;

            }
        }

        [HarmonyPatch(typeof(PackageUIBase), "Update")]
        static class StoreageUICtr_Update_Patch
        {
            static void Postfix(PackageUIBase __instance)
            {
                if (!enabled || UIStateMgr.Instance.currentState.type != UIStateMgr.StateType.Storeage || !(__instance is StoreageUICtr))
                    return;

                if (Input.GetKeyDown(settings.PrevStorageKey))
                {
                    (__instance as StoreageUICtr).SwitchStorage(false);
                }
                else if (Input.GetKeyDown(settings.NextStorageKey))
                {
                    (__instance as StoreageUICtr).SwitchStorage(true);
                }

            }
        }


    }
}
