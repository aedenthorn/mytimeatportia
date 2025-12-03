using Harmony12;
using Pathea;
using Pathea.OptionNs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace DialogueEdit
{
    public partial class Main
    {
        public static bool enabled;
        private static UnityModManager.ModEntry myModEntry;

        private static readonly bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "DialogueEdit " : "") + str);
        }
        public static Settings settings { get; private set; }

        public static Dictionary<int, string> dictStrings;
        public static Dictionary<string, string> replaceStrings;

        // Send a response to the mod manager about the launch status, success or not.
        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            myModEntry = modEntry;
            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            return true;
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

        private static void reloadStrings()
        {
            string lang = "";

            switch ((int)Singleton<OptionsMgr>.Instance.LanguageGame)
            {
                case 0:
                    lang = "Chinese";
                    break;
                case 1:
                    lang = "English";
                    break;
                case 2:
                    lang = "German";
                    break;
                case 3:
                    lang = "French";
                    break;
                case 4:
                    lang = "T_Chinese";
                    break;
                case 5:
                    lang = "Italian";
                    break;
                case 6:
                    lang = "Spanish";
                    break;
                case 7:
                    lang = "Japanese";
                    break;
                case 8:
                    lang = "Russian";
                    break;
                case 9:
                    lang = "Turkish";
                    break;
                case 10:
                    lang = "Korean";
                    break;
                case 11:
                    lang = "Portuguese";
                    break;
                case 12:
                    lang = "Vietnamese";
                    break;
            }
            if (lang == "")
                return;

            dictStrings = new Dictionary<int, string>();
            replaceStrings = new Dictionary<string, string>();
            string path = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\assets\\{lang}.txt";
            if (!File.Exists(path))
            {
                Dbgl("No translation file found at "+path);
                return;
            }
            var lines = File.ReadAllLines(path);
            for (var i = 0; i < lines.Length; i += 1)
            {
                string[] line = lines[i].Split('=');
                if (line.Length < 2)
                    continue;
                var text = new string[line.Length - 1];
                Array.Copy(line, 1, text, 0, line.Length - 1);
                if (int.TryParse(line[0], out int id))
                {
                    dictStrings.Add(id, text.Join());
                }
                else
                {
                    replaceStrings.Add(line[0], text.Join());
                }
            }
        }

    }
}
