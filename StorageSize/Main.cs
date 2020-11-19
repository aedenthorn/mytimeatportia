using Harmony12;
using Pathea.HomeNs;
using Pathea.ItemSystem;
using System;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace StorageSize
{
    public class Main
    {
        public static Settings settings { get; private set; }
        public static bool enabled;

        private static readonly bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? typeof(Main).Namespace + " " : "") + str);
        }


        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnToggle = OnToggle;
            modEntry.OnShowGUI = OnShowGUI;

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
            if (int.TryParse(tempWoodenSizeString, out int tempWoodenSize))
            {
                settings.WoodenStorageSize = tempWoodenSize;
            }
            if (int.TryParse(tempMetalSizeString, out int tempMetalSize))
            {
                settings.MetalStorageSize = tempMetalSize;
            }
            if (int.TryParse(tempSafetySizeString, out int tempSafetySize))
            {
                settings.SafetyBoxSize = tempSafetySize;
            }
            tempWoodenSizeString = "" + settings.WoodenStorageSize;
            tempMetalSizeString = "" + settings.MetalStorageSize;
            tempSafetySizeString = "" + settings.SafetyBoxSize;
            
            settings.Save(modEntry);
        }

        private static void OnShowGUI(UnityModManager.ModEntry obj)
        {
            tempWoodenSizeString = "" + settings.WoodenStorageSize;
            tempMetalSizeString = "" + settings.MetalStorageSize;
            tempSafetySizeString = "" + settings.SafetyBoxSize;
        }


        private static string tempWoodenSizeString;
        private static string tempMetalSizeString;
        private static string tempSafetySizeString;
        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label("Wooden Storage Size (5 per row, 30 per page):");
            tempWoodenSizeString = GUILayout.TextField(tempWoodenSizeString, new GUILayoutOption[0]);
            GUILayout.Space(10);

            GUILayout.Label("Metal Storage Size (5 per row, 30 per page):");
            tempMetalSizeString = GUILayout.TextField(tempMetalSizeString, new GUILayoutOption[0]);
            GUILayout.Space(10);

            GUILayout.Label("Safety Box Size (5 per row, 30 per page):");
            tempSafetySizeString = GUILayout.TextField(tempSafetySizeString, new GUILayoutOption[0]);
            GUILayout.Space(10);

            settings.UpdateExistingStorages = GUILayout.Toggle(settings.UpdateExistingStorages, "Update existing storages (can lead to loss of items if shrinking!)");
            GUILayout.Space(20);
        }

        [HarmonyPatch(typeof(StorageUnit), new Type[] { typeof(int) })]
        [HarmonyPatch(MethodType.Constructor)]
        internal static class StorageUnit_Patch
        {
            private static void Postfix(StorageUnit __instance)
            {
                if (!enabled)
                    return;
                Dbgl($"new box, level {__instance.Level}");
                int count;
                switch (__instance.Level)
                {
                    case 0:
                        count = settings.WoodenStorageSize;
                        break;
                    case 1:
                        count = settings.MetalStorageSize;
                        break;
                    case 2:
                    case -1:
                        count = settings.MetalStorageSize;
                        break;
                    default:
                        return;
                }
                Dbgl($"Changing slots for level {__instance.Level} to {count}");

                typeof(StorageUnit).GetMethod("InitStoreage", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { count, count });
            }
        }
        [HarmonyPatch(typeof(StorageUnit), "Storeage")]
        [HarmonyPatch(MethodType.Getter)]
        internal static class StorageUnit_Storeage_Patch
        {
            private static bool Prefix(StorageUnit __instance, ref ItemTable ___storage, ref ItemTable __result)
            {
                if (!enabled || !settings.UpdateExistingStorages)
                    return true;

                TableSlot[] slots = (TableSlot[])typeof(ItemTable).GetField("slots", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(___storage);

                int count = slots.Length;
                int lcount;
                switch (__instance.Level)
                {
                    case 0:
                        lcount = settings.WoodenStorageSize;
                        break;
                    case 1:
                        lcount = settings.MetalStorageSize;
                        break;
                    case 2:
                    case -1:
                        lcount = settings.MetalStorageSize;
                        break;
                    default:
                        return true;
                }

                if(count == lcount)
                    return true;

                Dbgl($"Changing slots for level {__instance.Level} from {count} to {lcount}");
                TableSlot[] newSlots = new TableSlot[lcount];
                Array.Copy(slots, newSlots, lcount > count ? count : lcount);
                if(lcount > count)
                {
                    for(int i = 0; i < lcount - count; i++)
                    {
                        newSlots[count + i] = new TableSlot();
                    }
                }
                typeof(ItemTable).GetField("slots", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(___storage, newSlots);
                typeof(ItemTable).GetField("unlockedCount", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(___storage, newSlots.Length);
                __result = ___storage;
                return false;
            }
        } 
    }
}
