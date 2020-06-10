using Harmony12;
using Pathea;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;
using Pathea.AnimalFarmNs;
using Pathea.HomeNs;
using Hont.ExMethod.Collection;
using Pathea.TipsNs;
using System.IO;
using Pathea.ItemSystem;
using System.Linq;

namespace Television
{
    public class Main
    {
        public static Settings settings { get; private set; }

        public static bool enabled;

        private static readonly bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "Television " : "") + str);
        }

        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnToggle = OnToggle;

            LoadVideoFiles();


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

        private static void LoadVideoFiles()
        {
            int id = 40032000;

            string path = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\assets";

            videoPaths.Clear();

            foreach (string file in Directory.GetFiles(path))
            {
                string name = Path.GetFileName(file);
                Dbgl("file: " + name);
                name = name.Substring(0, name.Length - 4);
                Dbgl("name: " + name);

                if (file.EndsWith(".mp4"))
                {
                    videoPaths.Add(name, file);
                    videoNames.Add(id++, name);
                }
                else if (file.EndsWith(".txt"))
                {
                    videoPaths.Add(name, File.ReadAllLines(file)[0]);
                    videoNames.Add(id++, name);
                }
            }
        }

        private static Dictionary<string, string> videoPaths = new Dictionary<string, string>();
        private static Dictionary<int, string> videoNames = new Dictionary<int, string>();

        [HarmonyPatch(typeof(VideoChoiceUICtr), "GetVideoes")]
        static class VideoChoiceUICtr_GetVideoes
        {
            static void Postfix(ref List<VideoTypeData> __result)
            {
                if (!enabled)
                    return;
                if (__result == null)
                {
                    __result = new List<VideoTypeData>();
                }
                foreach (KeyValuePair<int, string> kvp in videoNames)
                {
                    __result.Add(new VideoTypeData()
                    {
                        FileName = kvp.Value,
                        Id = kvp.Key
                    });;
                }

            }
        }



        [HarmonyPatch(typeof(ItemDataMgr), "OnLoad")]
        static class ItemDataMgr_OnLoad_Patch
        {
            static void Postfix(ItemDataMgr __instance, ref List<ItemBaseConfData> ___itemBaseList, ref List<EquipmentItemConfData> ___equipmentDataList)
            {
                if (!enabled)
                    return;

                foreach (KeyValuePair<int,string> kvp in videoNames)
                {
                    ItemBaseConfData itemConf = new ItemBaseConfData()
                    {
                        ID = kvp.Key,
                        NameID = kvp.Key,
                    };

                    ___itemBaseList.Add(itemConf);

                }
            }
        }

        [HarmonyPatch(typeof(TextMgr), "Get")]
        private static class TextMgr_Get_Patch
        {
            private static bool Prefix(int id, ref string __result)
            {
                if (!enabled)
                    return true;

                if (videoNames.ContainsKey(id))
                {
                    __result = videoNames[id];
                    return false;
                }
                return true;
            }
        }



        [HarmonyPatch(typeof(TVCtr), "Play")]
        static class TVCtr_Play
        {
            static bool Prefix(TVCtr __instance, string fileName)
            {
                if (!enabled)
                    return true;

                if (__instance.CurPlay == fileName)
                {
                    return false;
                }
                if (string.IsNullOrEmpty(fileName))
                {
                    __instance.Stop();
                    return false;
                }

                Dbgl("filename: " + fileName);

                if (videoPaths.ContainsKey(fileName))
                {
                    Dbgl("got file: " + videoPaths[fileName]);
                    try
                    {
                        typeof(TVCtr).GetMethod("PlayUrl", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { videoPaths[fileName] });
                    }
                    catch (Exception ex)
                    {
                        Dbgl("error: " + ex);
                    }
                    return false;
                }

                return true;
            }
        }
    }
}
