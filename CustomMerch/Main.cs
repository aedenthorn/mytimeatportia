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
        private static bool isDebug = false;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
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
                                storeItems.Add(new StoreItem(jsonNode["ID"].AsInt, productIds++, jsonNode["count"].AsInt, jsonNode["currency"].AsInt, jsonNode["requireMission"], jsonNode["storeId"].AsInt, jsonNode["chance"].AsFloat));
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
        }

        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }
    }
}