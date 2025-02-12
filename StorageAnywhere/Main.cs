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
using Pathea.UISystemNs.UIBase;
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
            GUILayout.Label("<b>Itembar Switch Key:</b>", new GUILayoutOption[0]);
            settings.ItemBarSwitchKey = GUILayout.TextField(settings.ItemBarSwitchKey, new GUILayoutOption[0]);
            GUILayout.Space(10);
            GUILayout.Label("<b>Open Storage Key:</b>", new GUILayoutOption[0]);
            settings.OpenStorageKey = GUILayout.TextField(settings.OpenStorageKey, new GUILayoutOption[0]);
            GUILayout.Space(10);
            GUILayout.Label("<b>Open Factory Storage Modifier Key:</b>", new GUILayoutOption[0]);
            settings.OpenFactoryModKey = GUILayout.TextField(settings.OpenFactoryModKey, new GUILayoutOption[0]);
            GUILayout.Space(10);
            GUILayout.Label("<b>Open Factory Product Modifier Key:</b>", new GUILayoutOption[0]);
            settings.OpenFactoryProductModKey = GUILayout.TextField(settings.OpenFactoryProductModKey, new GUILayoutOption[0]);
            GUILayout.Space(10);
            GUILayout.Label("<b>Switch to Prev Storage Key:</b>", new GUILayoutOption[0]);
            settings.PrevStorageKey = GUILayout.TextField(settings.PrevStorageKey, new GUILayoutOption[0]);
            GUILayout.Space(10);
            GUILayout.Label("<b>Switch to Next Storage Key:</b>", new GUILayoutOption[0]);
            settings.NextStorageKey = GUILayout.TextField(settings.NextStorageKey, new GUILayoutOption[0]);
            GUILayout.Space(10);
            GUILayout.Label("<b>Last Page Key:</b>", new GUILayoutOption[0]);
            settings.PrevPageKey = GUILayout.TextField(settings.PrevPageKey, new GUILayoutOption[0]);
            GUILayout.Label("Use shift to change storage page", new GUILayoutOption[0]);
            GUILayout.Space(10);
            GUILayout.Label("<b>Next Page Key:</b>", new GUILayoutOption[0]);
            settings.NextPageKey = GUILayout.TextField(settings.NextPageKey, new GUILayoutOption[0]);
            GUILayout.Label("Use shift to change storage page", new GUILayoutOption[0]);
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
        
        static bool KeyHeld(string key)
        {
            try
            {
                return (Input.GetKey(key));
            }
            catch
            {
                return false;
            }
        }

        [HarmonyPatch(typeof(PlayerItemBarCtr), "Update")]
        static class ItemBar_Patch
        {
            static void Prefix(PlayerItemBarCtr __instance)
            {
                if (!enabled)
                    return;

                if (KeyDown(settings.ItemBarSwitchKey))
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
                else if (KeyDown(settings.OpenStorageKey) && UIStateMgr.Instance.currentState.type == UIStateMgr.StateType.Play)
                {
                    if (KeyHeld(settings.OpenFactoryModKey) && UIStateMgr.Instance.currentState.type == UIStateMgr.StateType.Play)
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
                    else if (KeyHeld(settings.OpenFactoryProductModKey) && UIStateMgr.Instance.currentState.type == UIStateMgr.StateType.Play)
                    {
                        FarmFactory[] factorys = Module<FarmFactoryMgr>.Self.GetAllFactorys();

                        if (factorys.Length == 0)
                            return;
                        FarmFactory factory = factorys[0];
                        UIStateMgr.Instance.ChangeStateByType(UIStateMgr.StateType.FarmFactory, false, new object[]
                        {
                            FarmFactoryState.UIType.FinishProduct,
                            factory.FactoryId
                        });
                    }
                    else
                    {

                        StorageViewer sv = new StorageViewer();
                        FieldRef<StorageViewer, StorageUnit> suRef = FieldRefAccess<StorageViewer, StorageUnit>("storageUnit");
                        suRef(sv) = StorageUnit.GetStorageByGlobalIndex(lastStorageIndex);

                        MethodInfo dynMethod = sv.GetType().GetMethod("InteractStorage", BindingFlags.NonPublic | BindingFlags.Instance);
                        dynMethod.Invoke(sv, new object[] { });
                    }
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
            static void Postfix(PackageUIBase __instance, UIPageTurning ___bagPageTurning)
            {
                if (!enabled || Module<InputSolutionModule>.Self.CurSolutionType == SolutionType.ColorConfig)
                    return;

                bool shift = Input.GetKey("left shift");
                bool prevPage = KeyDown(settings.PrevPageKey);
                bool nextPage = KeyDown(settings.NextPageKey);

                if (__instance is StoreageUICtr)
                {
                    if (KeyDown(settings.PrevStorageKey))
                    {
                        (__instance as StoreageUICtr).SwitchStorage(false);
                        return;
                    }
                    else if (KeyDown(settings.NextStorageKey))
                    {
                        (__instance as StoreageUICtr).SwitchStorage(true);
                        return;
                    }
                }

                if (!shift)
                {
                    if (prevPage)
                    {
                        ___bagPageTurning.TurnPage(false);
                    }
                    else if (nextPage)
                    {
                        ___bagPageTurning.TurnPage(true);
                    }
                    return;
                }


                if (!prevPage && !nextPage)
                {
                    return;
                }


                UIPageTurning ___storagePageTurning = null;

                if (__instance is StoreageUIBase || __instance is StoreageUICtr)
                {
                    ___storagePageTurning = AccessTools.FieldRefAccess<StoreageUIBase, UIPageTurning>((__instance as StoreageUIBase), "storagePageTurning");
                }
                else if(__instance is PackageExchangeAutoSortUICtr || __instance is PackageExchangeUICtr)
                {
                    ___storagePageTurning = AccessTools.FieldRefAccess<PackageExchangeUICtr, UIPageTurning>((__instance as PackageExchangeUICtr), "storagePageTurning");

                }
                else if(__instance is Store_Npc_UI)
                {
                    ___storagePageTurning = AccessTools.FieldRefAccess<Store_Npc_UI, UIPageTurning>((__instance as Store_Npc_UI), "storePageTurning");
                }

                if (___storagePageTurning != null)
                {
                    if (prevPage)
                    {
                        ___storagePageTurning.TurnPage(false);
                    }
                    else if (nextPage)
                    {
                        ___storagePageTurning.TurnPage(true);
                    }
                }
            }
        }


    }
}
