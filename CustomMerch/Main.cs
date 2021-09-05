using Harmony12;
using Pathea.ItemSystem;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace CustomMerch
{
    public partial class Main
    {
        private static bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (settings.isDebug)
                Debug.Log((pref ? "CustomMerch " : "") + str);
        }
        public static bool enabled;
        public static Settings settings { get; private set; }

        public static Dictionary<int, string> nameDesc = new Dictionary<int, string>();
        public static Dictionary<int, int> sourceStrings = new Dictionary<int, int>();

        // Send a response to the mod manager about the launch status, success or not.
        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            LoadItems();

            //LoadItemFile();

            return true;
        }

        private static void LoadItems()
        {
            // weapons items
            string path = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\assets\\";
            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path, "*.json");
                foreach(string file in files)
                {
                    try
                    {
                        JSONNode jsonRoot = JSONNode.Parse(File.ReadAllText(file));
                        if (jsonRoot != null)
                        {
                            for (int i = 0; i < jsonRoot.Count; i++)
                            {
                                JSONNode jsonNode = jsonRoot[i];

                                int count = 1;
                                if (jsonNode["count"] != null)
                                    count = jsonNode["count"].AsInt;

                                int currency = -1;
                                if (jsonNode["currency"] != null)
                                    currency = jsonNode["currency"].AsInt;

                                string exchange = "-1";
                                if (jsonNode["exchange"] != null)
                                    exchange = jsonNode["exchange"];

                                string requireMission = "-1";
                                if (jsonNode["requireMission"] != null)
                                    requireMission = jsonNode["requireMission"];

                                float chance = 1f;
                                if (jsonNode["chance"] != null)
                                    chance = jsonNode["chance"].AsFloat;

                                Dbgl($"add from json: {jsonNode["ID"]} as productIds={productIds} to store={jsonNode["storeId"]} with count={count} price {exchange} @ {currency}, chance={chance}, requireMission={requireMission}");
                                storeItems.Add(new StoreItem(jsonNode["ID"].AsInt, productIds++, count, currency, exchange, requireMission, jsonNode["storeId"].AsInt, chance));
                            }
                        }
                    }
                    catch (Exception arg)
                    {
                        Debug.LogError("exception reading json file:" + arg);
                    }
                }
            }
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            settings.isDebug = GUILayout.Toggle(settings.isDebug, "Enable debug logs", new GUILayoutOption[0]);

        }

        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }
    }
}
