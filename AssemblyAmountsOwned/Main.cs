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
using Pathea.ItemBoxNs;
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

namespace AssemblyAmountsOwned
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
        
        [HarmonyPatch(typeof(CreationBoardUICtr), "Fresh")]
        static class CreationBoardUICtr_Fresh_Patch
        {
            static void Postfix(CreationBoardUICtr __instance, CreationStateDesc ___curInformation)
            {
                if (!enabled)
                    return;
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
                FarmFactory[] factorys = Module<FarmFactoryMgr>.Self.GetAllFactorys();

                if (factorys.Length > 0)
                {
                    for (var f = 0; f < factorys.Length; f++)
                    {

                        for (int j = factorys[f].FinishedList.Count - 1; j >= 0; j--)
                        {
                            if(factorys[f].FinishedList[j].id == idCount.id)
                            {
                                remaining -= factorys[f].FinishedList[j].count;
                                if (remaining <= 0)
                                    goto next;
                            }    
                        }
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

                FarmFactory[] factorys = Module<FarmFactoryMgr>.Self.GetAllFactorys();

                if (factorys.Length > 0)
                {
                    for (var f = 0; f < factorys.Length; f++)
                    {

                        for (int j = factorys[f].FinishedList.Count - 1; j >= 0; j--)
                        {
                            if (factorys[f].FinishedList[j].id == idCount.id)
                            {
                                int take = Math.Min(factorys[f].FinishedList[j].count, remaining);

                                remaining -= take;
                                factorys[f].FinishedList[j].count -= take;
                                if(factorys[f].FinishedList[j].count <= 0)
                                {
                                    factorys[f].FinishedList.RemoveAt(j);
                                }
                                if (remaining <= 0)
                                    goto next;
                            }
                        }
                    }
                }
                Dbgl($"Not enough to cover {idCount.id}");
            next:
                continue;
            }
        }
    }
}
