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
using Pathea.ItemSystem;
using Pathea.MessageSystem;
using Pathea.ModuleNs;
using Pathea.SpawnNs;
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

            //modEntry.OnGUI = OnGUI;
           // modEntry.OnSaveGUI = OnSaveGUI;
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
                if (!enabled || ___operationState > CPartOperation.Set || ___lockInteract)
                    return true;
                var curMaterialItem = ___refData.materials.Find((CreationMaterialItem it) => it.id == ___curMaterialItemId);
                foreach (CreationMaterialItem material in ___refData.materials)
                {
                    if (material.itemList != null && material != curMaterialItem && Module<Player>.Self.bag.CanRemoveItemList(material.itemList))
                    {
                        Dbgl("Allowed to try setting");
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
                if (!enabled || ___operationState > CPartOperation.Set)
                    return true;
                Dbgl("Checking for setting");
                List<CreationMaterialItem> list = ___refData?.materials;
                if (list != null && list.Count > 0)
                {
                    if (list.Count == 1)
                    {
                        if (list[0] != null && Module<Player>.Self.bag.CanRemoveItemList(list[0].itemList))
                        {
                            __result = (bool)AccessTools.Method(typeof(CreationPart), "SetPrefab").Invoke(__instance, new object[] { list[0] });
                            Dbgl($"setting 1");
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
                                Dbgl($"setting 2");
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
        }

    }
}
