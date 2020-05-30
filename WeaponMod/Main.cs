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
        private static bool isDebug = false;

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
                GUILayout.Label(string.Format("Special weapons store: {0}", storeNames.Values.ToArray()[settings.specialWeaponStore]), new GUILayoutOption[0]);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(40);
                settings.specialWeaponStore = (int)GUILayout.HorizontalSlider(settings.specialWeaponStore, 0f, storeNames.Count-1, new GUILayoutOption[0]);
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