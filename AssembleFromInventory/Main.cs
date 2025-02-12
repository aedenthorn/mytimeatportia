using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
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
using Pathea.TagNs;
using Pathea.UISystemNs;
using Pathea.UISystemNs.UIBase;
using PatheaScriptExt;
using UnityEngine;
using UnityModManagerNet;
using static Harmony12.AccessTools;

namespace AssembleFromInventory
{
    public class Main
    {
        private static readonly bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log($"{(pref ? typeof(Main).Namespace : "") } {str}");
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
            settings.PullFromStorage = GUILayout.Toggle(settings.PullFromStorage, "Pull From Storage", new GUILayoutOption[0]);
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
        
        [HarmonyPatch(typeof(CreationPart), nameof(CreationPart.CanTrySetItem))]
        static class CreationPart_CanTrySetItem_Patch
        {
            static bool Prefix(CreationPart __instance, CPartOperation ___operationState, int ___curMaterialItemId, CreationPartData ___refData, bool ___lockInteract, ref bool __result)
            {
                if (!enabled || ___operationState == CPartOperation.Replace_GetBack || ___operationState == CPartOperation.GetBack)
                    return true;
                if (___lockInteract)
                    return false;
                var curMaterialItem = ___refData.materials.Find((CreationMaterialItem it) => it.id == ___curMaterialItemId);
                foreach (CreationMaterialItem material in ___refData.materials)
                {

                    if (CanRemoveItemList(material.itemList))
                    {
                        __result = true;
                        return false;
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(CreationPart), "DoSet")]
        static class CreationPart_DoSet_Patch
        {
            static bool Prefix(CreationPart __instance, CPartOperation ___operationState, CreationPartData ___refData, ref bool __result)
            {
                if (!enabled || ___operationState == CPartOperation.Replace_GetBack || ___operationState == CPartOperation.GetBack)
                    return Module<Player>.Self.HandItem != null;
                List<CreationMaterialItem> list = ___refData?.materials;
                if (list != null && list.Count > 0)
                {
                    if (list.Count == 1)
                    {
                        if (list[0] != null && CanRemoveItemList(list[0].itemList))
                        {
                            __result = (bool)AccessTools.Method(typeof(CreationPart), "SetPrefab").Invoke(__instance, new object[] { list[0] });
                            return false;
                        }
                    }
                    else
                    {
                        foreach (CreationMaterialItem creationMaterialItem in list)
                        {
                            if (creationMaterialItem != null && Module<Player>.Self.bag.CanRemoveItemList(creationMaterialItem.itemList))
                            {
                                __result = (bool)AccessTools.Method(typeof(CreationPart), "SetPrefab").Invoke(__instance, new object[] { creationMaterialItem });
                                return false;
                            }
                        }
                    }
                }
                return Module<Player>.Self.HandItem != null;
            }
        }

        private static bool CanRemoveItemList(List<IdCount> itemList)
        {
            for (int k = 0; k < itemList.Count; k++)
            {
                var idCount = itemList[k];
                int remaining = idCount.count;
                if (idCount.id < 0)
                {
                    if (Module<Player>.Self.bag.Money < remaining)
                    {
                        return false;
                    }
                    continue;
                }
                var itemCount = Module<Player>.Self.bag.GetItemCount(idCount.id);
                remaining -= itemCount;
                if (remaining <= 0)
                    continue;
                if (!settings.PullFromStorage)
                    return false;
                List<StorageUnit> globalStorages = (List<StorageUnit>)AccessTools.Field(typeof(StorageUnit), "globalStorages").GetValue(null);
                for (int i = 0; i < globalStorages.Count; i++)
                {
                    for (int j = 0; j < globalStorages[i].Storeage.UnlockedCount; j++)
                    {
                        ItemObject itemObj = globalStorages[i].Storeage.GetItemObj(j);
                        if (itemObj == null || itemObj.ItemDataId != idCount.id)
                        {
                            continue;
                        }
                        remaining -= itemObj.Number;
                        if (remaining <= 0)
                            goto next;
                    }
                }
                return false;
            next:
                continue;
            }
            return true;
        }

        [HarmonyPatch(typeof(CreationPart), "SetPrefab")]
        public static class CreationPart_SetPrefab_Patch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                Dbgl($"Transpiling CreationPart.SetPrefab");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo && codes[i].operand == AccessTools.Method(typeof(ItemBag), nameof(ItemBag.RemoveItemListHandFirst)))
                    {
                        Dbgl($"Replacing ItemBag.RemoveItemListHandFirst");

                        codes[i].opcode = OpCodes.Call;
                        codes[i].operand = AccessTools.Method(typeof(Main), nameof(Main.RemoveItemListHandFirst));
                        break;
                    }
                }
                return codes.AsEnumerable();
            }
        }

        public static void RemoveItemListHandFirst(ItemBag bag, List<IdCount> itemList, bool showTips)
        {
            for (int k = 0; k < itemList.Count; k++)
            {
                var idCount = itemList[k];
                int remaining = idCount.count;
                // check hand
                if (bag.HandObj != null && bag.HandObj.ItemDataId == idCount.id)
                {
                    Dbgl($"item {bag.HandObj.ItemBase.Name} in hand {bag.HandObj.Number}/{remaining}");

                    int take = Math.Min(bag.HandObj.Number, remaining);
                    bag.RemoveItem(bag.HandObj, take, showTips, true);

                    remaining -= take;
                    if (remaining <= 0)
                        continue;
                }
                if (idCount.id > 0)
                {
                    var itemCount = bag.GetItemCount(idCount.id);
                    if (itemCount > 0)
                    {

                        Dbgl($"item {idCount.id} in bag {itemCount}/{remaining}");

                        int take = Math.Min(itemCount, remaining);
                        bag.RemoveItem(idCount.id, Math.Min(take, itemCount), showTips, true);

                        remaining -= take;
                        if (remaining <= 0)
                            continue;
                    }
                }
                else
                {
                    Dbgl($"item is money {remaining}");

                    bag.ChangeMoney(-remaining, showTips, 0);
                    continue;
                }
                if (!settings.PullFromStorage)
                {
                    Dbgl($"Not enough to cover {idCount.id}");
                    continue;
                }

                List<StorageUnit> globalStorages = (List<StorageUnit>)AccessTools.Field(typeof(StorageUnit), "globalStorages").GetValue(null);
                for (int i = 0; i < globalStorages.Count; i++)
                {
                    Singleton<UILoadingMaskMgr>.Instance.UpdateInfo(i + 1 + "/" + globalStorages.Count);
                    Singleton<UILoadingMaskMgr>.Instance.UpdateProgress(i / globalStorages.Count - 1);
                    for (int j = 0; j < globalStorages[i].Storeage.UnlockedCount; j++)
                    {
                        ItemObject itemObj = globalStorages[i].Storeage.GetItemObj(j);
                        if (itemObj == null || itemObj.ItemDataId != idCount.id)
                        {
                            continue;
                        }
                        Dbgl($"item {itemObj.ItemBase.Name} in storage {itemObj.Number}/{remaining}");

                        int take = Math.Min(itemObj.Number, remaining);
                        globalStorages[i].Storeage.RemoveItem(itemObj, take);
                        remaining -= take;
                        if (remaining == 0)
                            goto next;
                    }
                }
                Dbgl($"Not enough to cover {idCount.id}");
            next:
                continue;
            }
        }
    }
}
