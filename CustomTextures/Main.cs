using Harmony12;
using Hont.ExMethod.Collection;
using Pathea;
using Pathea.ActorNs;
using Pathea.ModuleNs;
using Pathea.NpcAppearNs;
using Pathea.NpcRepositoryNs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityModManagerNet;

namespace CustomTextures
{
    public partial class Main
    {
            
        private static bool isDebug = true;
        private static Dictionary<int, Texture2D> customTextures = new Dictionary<int, Texture2D>();
        private static Dictionary<int, Dictionary<int, Texture2D>> customTexturesSupp = new Dictionary<int, Dictionary<int, Texture2D>>();

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "Custom NPCs " : "") + str);
        }
        public static bool enabled;

        public static Settings settings { get; private set; }

        private static void Load(UnityModManager.ModEntry modEntry)
        {

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            settings = Settings.Load<Settings>(modEntry);

            LoadCustomTextures();

        }

        private static void LoadCustomTextures()
        {
            string path = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\assets";

            if (!Directory.Exists(path))
            {
                Dbgl($"Directory {path} does not exist!");
                return;
            }

            Regex pattern = new Regex(@"^[0-9][0-9][0-9][0-9][0-9][0-9][0-9]\.png$");
            Regex pattern2 = new Regex(@"^[0-9][0-9][0-9][0-9][0-9][0-9][0-9]_[0-9][0-9]*\.png$");

            customTextures.Clear();
            customTexturesSupp.Clear();

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
                else if (pattern2.IsMatch(fileName))
                {
                    Dbgl($"got file at path: {file}");
                    string name = fileName.Substring(0, fileName.Length - 4);
                    int id = int.Parse(name.Substring(0, 7));
                    int idx = int.Parse(name.Substring(8));
                    Texture2D tex = new Texture2D(2, 2);
                    byte[] imageData = File.ReadAllBytes(file);
                    tex.LoadImage(imageData);
                    if (customTexturesSupp.ContainsKey(id))
                    {
                        customTexturesSupp[id].Add(idx, tex);
                    }
                    else
                    {
                        customTexturesSupp.Add(id, new Dictionary<int, Texture2D>() { { idx, tex } });
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
            if (GUILayout.Button("Reload Textures", new GUILayoutOption[]{
                GUILayout.Width(150f)
            }))
            {
                ReloadTextures();
            }
        }

        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }

        private static void ReloadTextures()
        {
            LoadCustomTextures();
            foreach (KeyValuePair<int,Texture2D> kvp in customTextures)
            {
                Actor actor = Module<ActorMgr>.Self.Get(kvp.Key);
                if (actor == null)
                    continue;
                NpcAppear appear = actor.GetComponent<NpcAppear>();
                if (appear != null)
                {
                    appear.RebuildMesh();
                }
                else
                {
                    SkinnedMeshRenderer smr = (SkinnedMeshRenderer)typeof(Actor).GetField("skinnedMeshRenderer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(actor);
                    if (smr != null)
                    {
                        smr.material.mainTexture = kvp.Value;
                    }
                }
            }
            foreach(KeyValuePair<int, Dictionary<int,Texture2D>> kvp in customTexturesSupp)
            {
                Actor actor = Module<ActorMgr>.Self.Get(kvp.Key);
                if (actor == null)
                    continue;
                NpcAppear appear = actor.GetComponent<NpcAppear>();
                if (appear != null)
                {
                    appear.RebuildMesh();
                }
            }
        }

        [HarmonyPatch(typeof(Actor), "Start")]
        private static class Actor_Start_Patch
        {
            private static void Postfix(Actor __instance, ref SkinnedMeshRenderer ___skinnedMeshRenderer)
            {

                if (customTextures.ContainsKey(__instance.InstanceId) && __instance.GetComponent<NpcAppear>() == null)
                {
                    if (__instance.InstanceId == 4000006)
                    {
                        ___skinnedMeshRenderer.sharedMesh = 
                    }

                    Dbgl($"got texture for {__instance.ActorName}");
                    if (___skinnedMeshRenderer != null)
                        ___skinnedMeshRenderer.material.mainTexture = customTextures[__instance.InstanceId];
                }
            }
        }
        [HarmonyPatch(typeof(NpcAppear), "RebuildMesh", new Type[] { typeof(List<NpcAppearUnit>), typeof(Transform), typeof(SkinnedMeshRenderer[]) })]
        private static class NpcAppear_RebuildMesh_Patch
        {
            private static void Prefix(NpcAppear __instance, List<NpcAppearUnit> npcAppearUnits, Actor ___m_Actor)
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
                        Dbgl($"got mesh for: {___m_Actor.ActorName}");
                        unit.smrs[0].material.mainTexture = customTextures[___m_Actor.InstanceId];
                    }
                }
                else if (customTexturesSupp.ContainsKey(___m_Actor.InstanceId))
                {
                    int i = 1;
                    foreach (NpcAppearUnit unit in npcAppearUnits)
                    {
                        if (unit == null || unit.smrs == null)
                            continue;
                        Dbgl($"getting mesh for: {___m_Actor.ActorName} at {i}");
                        if (customTexturesSupp[___m_Actor.InstanceId].ContainsKey(i))
                        {
                            Dbgl($"got mesh for: {___m_Actor.ActorName} at {i}");
                            unit.smrs[0].material.mainTexture = customTexturesSupp[___m_Actor.InstanceId][i];
                        }
                        i++;
                    }
                }
                
            }
        }
    }
}
