using Harmony12;
using Pathea;
using Pathea.ACT;
using Pathea.AudioNs;
using Pathea.CameraSystemNs;
using Pathea.ConfigNs;
using Pathea.MiniGameNs;
using Pathea.MiniGameNs.Fishing;
using Pathea.ModuleNs;
using Pathea.PhotoNs;
using Pathea.PlayerOperateNs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityModManagerNet;

namespace PhotoAlbum
{
    public class Main
    {
        public static Settings settings { get; private set; }
        public static bool enabled;
        private static List<PhotoData> customPhotos = new List<PhotoData>();

        private static readonly bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? typeof(Main).Namespace + " " : "") + str);
        }

        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnToggle = OnToggle;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id); 
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            LoadCustomPhotos();
        }

        private static void LoadCustomPhotos()
        {
            string path = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\assets";

            if (!Directory.Exists(path))
            {
                Dbgl($"Directory {path} does not exist!");
                return;
            }
            customPhotos.Clear();

            Regex pattern = new Regex(@"(png|jpg|jpeg|bmp)$");


            foreach (string file in Directory.GetFiles(path))
            {
                string fileName = Path.GetFileName(file);
                if (!pattern.IsMatch(fileName))
                    continue;

                Texture2D tex = new Texture2D(2, 2);
                byte[] imageData = File.ReadAllBytes(file);
                tex.LoadImage(imageData);
                PhotoData pd = new PhotoData();
                pd.filePath = file;
                pd.fileName = fileName;
                pd.realTime = new DateTime();
                pd.actorsId = new int[0];
                pd.comments = "";
                pd.bytes = imageData;
                customPhotos.Add(pd);
            }

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


        [HarmonyPatch(typeof(PhotoAlbumData), nameof(PhotoAlbumData.GetMaxSize))]
        static class PhotoAlbumData_GetMaxSize_Patch
        {
            static bool Prefix(PhotoAlbumData __instance, ref int __result)
            {
                if (!enabled || __instance.abumType != PhotoAlbumType.Other)
                    return true;

                __result = 1000;
                return false;
            }
        }


        [HarmonyPatch(typeof(PhotoAlbumMgr), nameof(PhotoAlbumMgr.GetAlbumByType))]
        static class PhotoAlbumMgr_GetAlbumByType_Patch
        {

            static void Postfix(PhotoAlbumMgr __instance, PhotoAlbumType type, ref PhotoAlbumData __result)
            {
                if (!enabled || type != PhotoAlbumType.Other || __result == null)
                    return;

                __result.photos = customPhotos;
            }
        }

    }
}
