using Harmony12;
using Pathea;
using Pathea.ACT;
using Pathea.ActorNs;
using Pathea.ItemSystem;
using Pathea.Missions;
using Pathea.ModuleNs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;
using static Pathea.ActorMotor;

namespace ItemSpawner
{
    public class Main
    {
        public static Settings settings { get; private set; }
        public static bool enabled;
        private static float standardWidth = 200f;
        private static float halfWidth = 100f;
        private static float shortWidth = 50f;
        private static string itemTemp;
        private static string numTemp = "1";
        private static string message = "";
        private static readonly bool isDebug = false;
        private static List<string> items = new List<string>();
        private static Dictionary<string, int> allItems = new Dictionary<string, int>();

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "ItemSpawner " : "") + str);
        }

        public static void Load(UnityModManager.ModEntry modEntry)
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
            if (!string.IsNullOrEmpty(message))
            {
                GUILayout.Label(message);
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("Item", new GUILayoutOption[] { GUILayout.Width(halfWidth) });
            var newItemTemp = GUILayout.TextField(itemTemp, new GUILayoutOption[] { GUILayout.Width(standardWidth) });
            if (newItemTemp != itemTemp)
            {
                message = "";
                itemTemp = newItemTemp;
                RebuildList();
            }
            GUILayout.Label("Amount", new GUILayoutOption[] { GUILayout.Width(halfWidth) });
            numTemp = GUILayout.TextField(numTemp, new GUILayoutOption[] { GUILayout.Width(shortWidth) });
            if (!string.IsNullOrEmpty(itemTemp) && int.TryParse(numTemp, out var num) && GUILayout.Button("Spawn", new GUILayoutOption[] { GUILayout.Width(halfWidth) }))
            {
                
                message = SpawnItem(num);
            }
            GUILayout.EndHorizontal();
            foreach (var s in items)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(halfWidth);
                if (GUILayout.Button(s, new GUILayoutOption[] { GUILayout.Width(standardWidth) }))
                {
                    itemTemp = s;
                    items.Clear();
                    items.Add(s);
                    break;
                }
                GUILayout.EndHorizontal();
            }
        }

        private static string SpawnItem(int num)
        {
            if (!allItems.TryGetValue(itemTemp, out var id))
            {
                return $"Item {itemTemp} not found.";
            }
            if (Module<Player>.Self?.bag == null)
            {
                return $"Player bag not found.";
            }
            if(num <= 0)
            {
                return $"Can't spawn {num} items.";
            }
            Module<Player>.Self.bag.AddItem(id, num, true, AddItemMode.Default);
            return $"Spawned {num} {itemTemp}.";
        }

        private static void RebuildList()
        {
            items.Clear();
            var lower = itemTemp.ToLower();
            items.AddRange(allItems.Keys.Where(s => s.ToLower().Contains(lower)));
        }
        
        [HarmonyPatch(typeof(ItemDataMgr), "OnLoad")]
        public static class ItemDataMgr_OnLoad_Patch
        {
            public static void Postfix(List<ItemBaseConfData> ___itemBaseList)
            {

                foreach (var i in ___itemBaseList)
                {
                    allItems[TextMgr.GetStr(i.NameID, -1)] = i.ID;
                }
                Dbgl($"Loaded {allItems.Count} items");
            }
        }
    }
}
