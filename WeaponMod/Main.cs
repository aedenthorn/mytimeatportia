using Harmony12;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace WeaponMod
{
    public partial class Main
    {
        private static bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "WeaponMod " : "") + str);
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

            LoadWeaponFile();

            return true;
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            settings.includeSpecialWeapons = GUILayout.Toggle(settings.includeSpecialWeapons, "Add special ingame weapons to store", new GUILayoutOption[0]);
            if (settings.includeSpecialWeapons)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(40);
                GUILayout.BeginVertical();
                GUILayout.Label(string.Format("Special weapons store: {0}", storeNames[settings.specialWeaponStore]), new GUILayoutOption[0]);
                settings.specialWeaponStore = (int)GUILayout.HorizontalSlider(settings.specialWeaponStore, 0f, storeNames.Count-1, new GUILayoutOption[0]);
                GUILayout.Space(10);
                GUILayout.Label(string.Format("Water Sword Chance: {0}", settings.waterSwordChance * 100), new GUILayoutOption[0]);
                settings.waterSwordChance = (int)GUILayout.HorizontalSlider(settings.waterSwordChance * 100, 0f, 100f, new GUILayoutOption[0]) / 100f;
                GUILayout.Space(10);
                GUILayout.Label(string.Format("Purple Haze Chance: {0}", settings.purpleHazeChance * 100), new GUILayoutOption[0]);
                settings.purpleHazeChance = (int)GUILayout.HorizontalSlider(settings.purpleHazeChance * 100, 0f, 100f, new GUILayoutOption[0]) / 100f;
                GUILayout.Space(10);
                GUILayout.Label(string.Format("Inflatable Hammer Chance: {0}", settings.InflateableHammerChance * 100), new GUILayoutOption[0]);
                settings.InflateableHammerChance = (int)GUILayout.HorizontalSlider(settings.InflateableHammerChance * 100, 0f, 100f, new GUILayoutOption[0]) / 100f;
                GUILayout.Space(10);
                GUILayout.Label(string.Format("Dev's Dagger Chance: {0}", settings.DevDaggerChance * 100), new GUILayoutOption[0]);
                settings.DevDaggerChance = (int)GUILayout.HorizontalSlider(settings.DevDaggerChance * 100, 0f, 100f, new GUILayoutOption[0]) / 100f;
                GUILayout.Space(10);
                GUILayout.Label(string.Format("??? Chance: {0}", settings.unknownWeaponChance * 100), new GUILayoutOption[0]);
                settings.unknownWeaponChance = (int)GUILayout.HorizontalSlider(settings.unknownWeaponChance * 100, 0f, 100f, new GUILayoutOption[0]) / 100f;
                GUILayout.Space(10);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }

        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }
        private static void LoadWeaponFile()
        {

            // weapons items
            string path = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\assets\\weaponItems.json";

            try
            {
                JSONNode jsonRoot = JSONNode.Parse(File.ReadAllText(path));
                if (jsonRoot != null)
                {
                    for (int i = 0; i < jsonRoot.Count; i++)
                    {
                        JSONNode jsonNode = jsonRoot[i];
                        WeaponItem item = new WeaponItem(jsonNode);
                        weaponItems.Add(item);
                    }
                }
            }
            catch (Exception arg)
            {
                Debug.LogError("handled exception:" + arg);
            }
        }
    }
}