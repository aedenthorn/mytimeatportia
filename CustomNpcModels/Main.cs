using Harmony12;
using Pathea.ActorNs;
using Pathea.ModuleNs;
using Pathea.NpcAppearNs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;
using static Harmony12.AccessTools;

namespace CustomNpcModels
{
    public class Main
    {

        private static bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "Custom Textures " : "") + str);
        }
        public static bool enabled;

        public static Settings settings { get; private set; }

        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            //Module<ScenarioModule>.Self.EndLoadEventor += OnLoadGame;

            //SceneManager.activeSceneChanged += ChangeScene;
            /*
            assetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/untitled");
            if (assetBundle != null)
            {
            }
            */

            //string[] names = { "Emily", "Nora", "Phyllis", "Ginger", "Sonia", };

            string path = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\assets";

            if (!Directory.Exists(path))
            {
                Dbgl($"Directory {path} does not exist!");
                return;
            }

            Regex pattern = new Regex(@"^[0-9][0-9]*\.png$");

            foreach (string file in Directory.GetFiles(path))
            {
                string fileName = Path.GetFileName(file);
                if (pattern.IsMatch(fileName))
                {
                    int id = int.Parse(fileName.Substring(0, fileName.Length - 4));
                    Dbgl($"got file at path: {file}");
                    Texture2D tex = new Texture2D(2, 2);
                    byte[] imageData = File.ReadAllBytes(file);
                    tex.LoadImage(imageData);
                    customTextures.Add(id, tex);
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


        private static Dictionary<int, Texture2D> customTextures = new Dictionary<int, Texture2D>();

        [HarmonyPatch(typeof(Actor), "Start")]
        private static class Actor_Start_Patch
        {
            private static void Postfix(ref Actor __instance, ref SkinnedMeshRenderer ___skinnedMeshRenderer)
            {
                if (customTextures.ContainsKey(__instance.InstanceId))
                {
                    if (___skinnedMeshRenderer != null)
                        ___skinnedMeshRenderer.material.mainTexture = customTextures[__instance.InstanceId];
                }
            }
        }
        [HarmonyPatch(typeof(NpcAppear), "RebuildMesh", new Type[] { typeof(List<NpcAppearUnit>), typeof(Transform), typeof(SkinnedMeshRenderer[]) })]
        private static class NpcAppear_RebuildMesh_Patch
        {
            private static void Prefix(NpcAppear __instance, ref List<NpcAppearUnit> npcAppearUnits, Actor ___m_Actor)
            {
                if (___m_Actor == null)
                {
                    Dbgl($"no actor");
                    return;
                }
                if (npcAppearUnits == null)
                {
                    Dbgl($"no npcAppearUnits");
                    return;
                }

                if (customTextures.ContainsKey(___m_Actor.InstanceId))
                {
                    foreach (NpcAppearUnit unit in npcAppearUnits)
                    {
                        if (unit == null || unit.smrs == null)
                            continue;
                        foreach (SkinnedMeshRenderer meshRenderer in unit.smrs)
                        {
                            if (meshRenderer != null && meshRenderer.material != null)
                            {
                                Dbgl($"got mesh for: {___m_Actor.ActorName}");
                                meshRenderer.material.mainTexture = customTextures[___m_Actor.InstanceId];
                            }
                        }
                    }
                }
            }
        }
    }
}
