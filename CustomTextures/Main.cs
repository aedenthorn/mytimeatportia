using Harmony12;
using Pathea;
using Pathea.ActorNs;
using Pathea.AppearNs;
using Pathea.HomeNs;
using Pathea.HomeViewerNs;
using Pathea.ModuleNs;
using Pathea.NpcAppearNs;
using Pathea.ScenarioNs;
using Pathea.SeasonNs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;

namespace CustomTextures
{
    public partial class Main
    {

        private static bool isDebug = true;
        private static Dictionary<int, Texture2D> customTextures = new Dictionary<int, Texture2D>();
        private static Dictionary<int, Dictionary<int, Texture2D>> customTexturesPartial = new Dictionary<int, Dictionary<int, Texture2D>>();
        private static Dictionary<int, Dictionary<string, Texture2D>> customTexturesExact = new Dictionary<int, Dictionary<string, Texture2D>>();
        private static Dictionary<string, Texture2D> customTexturesMisc = new Dictionary<string, Texture2D>();

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "Custom Textures " : "") + str);
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
            SceneManager.activeSceneChanged += ChangeScene;
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
            Regex pattern2 = new Regex(@"^[0-9][0-9][0-9][0-9][0-9][0-9][0-9]_[0-9]\.png$");
            Regex pattern3 = new Regex(@"^[0-9][0-9][0-9][0-9][0-9][0-9][0-9]_..*\.png$");

            customTextures.Clear();
            customTexturesPartial.Clear();
            customTexturesExact.Clear();
            customTexturesMisc.Clear();

            foreach (string file in Directory.GetFiles(path))
            {
                string fileName = Path.GetFileName(file);
                if (pattern.IsMatch(fileName))
                {
                    int id = int.Parse(fileName.Substring(0, fileName.Length - 4));
                    Dbgl($"got simple mesh texture at path: {file}");
                    Texture2D tex = new Texture2D(2, 2);
                    byte[] imageData = File.ReadAllBytes(file);
                    tex.LoadImage(imageData);
                    customTextures.Add(id, tex);
                }
                else if (pattern2.IsMatch(fileName))
                {
                    Dbgl($"got partial mesh texture at path: {file}");
                    string name = fileName.Substring(0, fileName.Length - 4);
                    int id = int.Parse(name.Substring(0, 7));
                    int idx = int.Parse(name.Substring(8));
                    Texture2D tex = new Texture2D(2, 2);
                    byte[] imageData = File.ReadAllBytes(file);
                    tex.LoadImage(imageData);
                    if (customTexturesPartial.ContainsKey(id))
                    {
                        customTexturesPartial[id].Add(idx, tex);
                    }
                    else
                    {
                        customTexturesPartial.Add(id, new Dictionary<int, Texture2D>() { { idx, tex } });
                    }
                }
                else if (pattern3.IsMatch(fileName))
                {
                    Dbgl($"got exact mesh texture at path: {file}");
                    string name = fileName.Substring(0, fileName.Length - 4);
                    int id = int.Parse(name.Split('_')[0]);
                    string mesh = name.Substring(8);
                    Texture2D tex = new Texture2D(2, 2);
                    byte[] imageData = File.ReadAllBytes(file);
                    tex.LoadImage(imageData);
                    if (customTexturesExact.ContainsKey(id))
                    {
                        customTexturesExact[id].Add(mesh, tex);
                    }
                    else
                    {
                        customTexturesExact.Add(id, new Dictionary<string, Texture2D>() { { mesh, tex } });
                    }
                }
                else
                {
                    Dbgl($"got misc texture at path: {file}");
                    Texture2D tex = new Texture2D(2, 2);
                    byte[] imageData = File.ReadAllBytes(file);
                    tex.LoadImage(imageData);
                    customTexturesMisc.Add(fileName.Substring(0, fileName.Length - 4), tex);
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
                GUILayout.Width(250f),
                GUILayout.Height(80f),
            }))
            {
                ReloadTextures();
            }
            if (GUILayout.Button("Dump Scene Names", new GUILayoutOption[]{
                GUILayout.Width(250f),
                GUILayout.Height(80f)
            }))
            {
                DumpObjectNames();
                UnityModManager.OpenUnityFileLog();
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
            foreach (KeyValuePair<int, Texture2D> kvp in customTextures)
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
            foreach (KeyValuePair<int, Dictionary<int, Texture2D>> kvp in customTexturesPartial)
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
            FixSceneTextures(SceneManager.GetActiveScene());
        }

        [HarmonyPatch(typeof(Actor), "Start")]
        private static class Actor_Start_Patch
        {
            private static void Postfix(Actor __instance, ref SkinnedMeshRenderer ___skinnedMeshRenderer)
            {

                if (customTextures.ContainsKey(__instance.InstanceId) && __instance.GetComponent<NpcAppear>() == null)
                {
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
                        //Dbgl($"got mesh for: {___m_Actor.ActorName}");
                        unit.smrs[0].material.mainTexture = customTextures[___m_Actor.InstanceId];
                    }
                }
                else if (customTexturesExact.ContainsKey(___m_Actor.InstanceId))
                {
                    foreach (NpcAppearUnit unit in npcAppearUnits)
                    {
                        if (unit == null || unit.smrs == null)
                            continue;
                        Dbgl($"mesh {unit.smrs[0].name}");
                        if (customTexturesExact[___m_Actor.InstanceId].ContainsKey(unit.smrs[0].name))
                        {
                            unit.smrs[0].material.mainTexture = customTexturesExact[___m_Actor.InstanceId][unit.smrs[0].name];
                        }
                    }
                }
                else if (customTexturesPartial.ContainsKey(___m_Actor.InstanceId))
                {
                    int i = 1;
                    foreach (NpcAppearUnit unit in npcAppearUnits)
                    {
                        if (unit == null || unit.smrs == null)
                            continue;
                        if (customTexturesPartial[___m_Actor.InstanceId].ContainsKey(i))
                        {
                            //Dbgl($"got mesh for: {___m_Actor.ActorName} at {i}");
                            unit.smrs[0].material.mainTexture = customTexturesPartial[___m_Actor.InstanceId][i];
                        }
                        i++;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(AppearTarget), "BuildMesh", new Type[] { typeof(List<AppearUnit>), typeof(AppearData), typeof(AppearUnit), typeof(string) })]
        static class BuildMesh1_Patch
        {
            static void Prefix(AppearTarget __instance, AppearData appearData, ref List<AppearUnit> units)
            {
                //Dbgl($"Building player mesh");

                if (!enabled)
                    return;

                for (int i = 0; i < units.Count; i++)
                {
                    string name = units[i].name;
                    Dbgl($"appear part name: {name}");
                    if (customTexturesMisc.ContainsKey(name))
                    {
                        Dbgl($"Changing texture for {name}");
                        units[i].Smr.material.mainTexture = customTexturesMisc[name];
                    }
                }
            }
        }

        private static void ChangeScene(Scene arg0, Scene arg1)
        {
            if (!enabled)
            {
                return;
            }
            FixSceneTextures(arg1);

            if(arg1.name == "Main")
            {
                FixWorkshopTextures();
            }
            else if(arg1.name == "PlayerHome")
            {
                FixHomeTextures();
            }
        }

        private static void FixWorkshopTextures()
        {
            LayeredRegion layeredRegion = (LayeredRegion)typeof(FarmModule).GetField("layeredRegion", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Module<FarmModule>.Self);
            Region itemLayer = (Region)typeof(LayeredRegion).GetField("itemLayer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(layeredRegion);
            List<object> slots = AccessTools.FieldRefAccess<Region, List<object>>(itemLayer, "slots");

            foreach (object slot in slots)
            {
                UnitObjInfo unitObjInfo = (UnitObjInfo)slot.GetType().GetField("unitObjInfo").GetValue(slot);
                GameObject go = unitObjInfo.go;
                FixOneTexture(go);

            }
        }

        private static void FixHomeTextures()
        {
            Dictionary<string, LayeredRegion> layeredRegions = (Dictionary<string, LayeredRegion>)typeof(WallModule).GetField("walls", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Module<WallModule>.Self);

            foreach(LayeredRegion layeredRegion in layeredRegions.Values)
            {
                Region itemLayer = (Region)typeof(LayeredRegion).GetField("itemLayer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(layeredRegion);
                List<object> slots = AccessTools.FieldRefAccess<Region, List<object>>(itemLayer, "slots");

                foreach (object slot in slots)
                {
                    UnitObjInfo unitObjInfo = (UnitObjInfo)slot.GetType().GetField("unitObjInfo").GetValue(slot);
                    GameObject go = unitObjInfo.go;
                    FixOneTexture(go);

                }
            }
        }

        private static void FixSceneTextures(Scene arg1)
        {
            GameObject[] gameObjects = arg1.GetRootGameObjects();
            foreach (GameObject obj in gameObjects)
            {
                FixOneTexture(obj);
            }
        }

        [HarmonyPatch(typeof(RegionViewer), "CreateGameObj", new Type[] { typeof(string), typeof(Area), typeof(ItemPutInfo), typeof(bool) })]
        static class RegionViewer_CreateGameObj_Patch
        {
            static void Postfix(GameObject __result)
            {
                if (!enabled)
                    return;

                FixOneTexture(__result);
            }
        }

        [HarmonyPatch(typeof(HomeItemSelector), "ShowPreviewObj")]
        static class HomeItemSelector_ShowPreviewObj_Patch
        {
            static void Postfix(Area ___area, ref GameObject ___previewGameObj)
            {
                if (!enabled || ___area == null || ___previewGameObj == null)
                    return;
                FixOneTexture(___previewGameObj);
            }
        }

        private static void FixOneTexture(GameObject go)
        {
            if (go == null)
                return;
            MeshRenderer[] mrs = go.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer mr in mrs)
            {
                string name = mr.name;
                if (customTexturesMisc.ContainsKey(name) && mr.material && mr.material.HasProperty("_MainTex") && mr.material.mainTexture != null)
                {
                    Dbgl($"Changing texture for {name}");
                    mr.material.mainTexture = customTexturesMisc[name];
                }
            }
        }

        private static void DumpObjectNames()
        {
            Scene scene = SceneManager.GetActiveScene();

            if(scene == null)
            {
                Dbgl("scene is null");
                return;
            }

            List<string> meshTextures = new List<string>();

            string names = "";

            names += $"DLC actor mesh and texture names:\r\n\r\n\t";

            List<AppearDBData> list = DbReader.Read<AppearDBData>(LocalDb.cur.ReadFullTable("NPCAppear_List"), 20);
            foreach (AppearDBData data in list)
            {
                string path = data.appearPath;
                NpcAppearUnit unit = Singleton<ResMgr>.Instance.LoadSyncByType<NpcAppearUnit>(AssetType.NpcAppear, path);
                foreach (SkinnedMeshRenderer mr in unit.smrs)
                {
                    if (mr.material && mr.material.HasProperty("_MainTex") && mr.material.mainTexture != null)
                    {
                        string mt = $"mesh name: {mr.name} texture name: {mr.material.mainTexture.name}";
                        if (!meshTextures.Contains(mt))
                        {
                            meshTextures.Add(mt);
                        }

                    }
                }
            }

            names += string.Join("\r\n\t", meshTextures.ToArray()) + "\r\n\r\n";
            meshTextures.Clear();

            Dbgl("got dlc actor meshes");

            /*
            names += $"Non-DLC actor mesh and texture names for scene {scene.name}:\r\n\r\n";

            Module<ActorMgr>.Self.Foreach(delegate (Actor actor) {
                if (actor.SceneName == scene.name && actor.GetComponent<NpcAppear>() == null)
                {
                    GameObject go = actor.modelRoot.transform.parent.gameObject;
                    SkinnedMeshRenderer[] smrs = go.GetComponentsInChildren<SkinnedMeshRenderer>();
                    foreach (SkinnedMeshRenderer mr in smrs)
                    {
                        if (mr.material && mr.material.HasProperty("_MainTex"))
                        {
                            names += $"\tmesh name: {mr.name} texture name: {mr.material.mainTexture.name}\r\n";
                        }
                    }
                }
            });
            names += $"\r\n\r\n";
            */
            GameObject[] gameObjects = scene.GetRootGameObjects();
            foreach (GameObject obj in gameObjects)
            {
                MeshRenderer[] mrs = obj.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer mr in mrs)
                {
                    if (mr.material && mr.material.HasProperty("_MainTex") && mr.material.mainTexture != null)
                    {
                        string mt = $"mesh name: {mr.name} texture name: {mr.material.mainTexture.name}";
                        if (!meshTextures.Contains(mt))
                        {
                            meshTextures.Add(mt);
                        }

                    }
                }
            }


            Dbgl("got scene objects");


            if (scene.name == "Main")
            {
                LayeredRegion layeredRegion = (LayeredRegion)typeof(FarmModule).GetField("layeredRegion", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Module<FarmModule>.Self);
                Region itemLayer = (Region)typeof(LayeredRegion).GetField("itemLayer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(layeredRegion);
                List<object> slots = AccessTools.FieldRefAccess<Region, List<object>>(itemLayer, "slots");

                foreach (object slot in slots)
                {
                    UnitObjInfo unitObjInfo = (UnitObjInfo)slot.GetType().GetField("unitObjInfo").GetValue(slot);
                    GameObject go = unitObjInfo.go;
                    if (go == null)
                        break;
                    MeshRenderer[] mrs = go.GetComponentsInChildren<MeshRenderer>();
                    foreach (MeshRenderer mr in mrs)
                    {
                        if (mr.material && mr.material.HasProperty("_MainTex") && mr.material.mainTexture != null)
                        {
                            string mt = $"mesh name: {mr.name} texture name: {mr.material.mainTexture.name}";
                            if (!meshTextures.Contains(mt))
                            {
                                meshTextures.Add(mt);
                            }

                        }
                    }
                }
            }
            else if (scene.name == "PlayerHome")
            {
                Dictionary<string, LayeredRegion> layeredRegions = (Dictionary<string, LayeredRegion>)typeof(WallModule).GetField("walls", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Module<WallModule>.Self);

                foreach (LayeredRegion layeredRegion in layeredRegions.Values)
                {
                    Region itemLayer = (Region)typeof(LayeredRegion).GetField("itemLayer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(layeredRegion);
                    List<object> slots = AccessTools.FieldRefAccess<Region, List<object>>(itemLayer, "slots");

                    foreach (object slot in slots)
                    {
                        UnitObjInfo unitObjInfo = (UnitObjInfo)slot.GetType().GetField("unitObjInfo").GetValue(slot);
                        GameObject go = unitObjInfo.go;
                        if (go == null)
                            break;
                        MeshRenderer[] mrs = go.GetComponentsInChildren<MeshRenderer>();
                        foreach (MeshRenderer mr in mrs)
                        {
                            if (mr.material && mr.material.HasProperty("_MainTex") && mr.material.mainTexture != null)
                            {
                                string mt = $"mesh name: {mr.name} texture name: {mr.material.mainTexture.name}";
                                if (!meshTextures.Contains(mt))
                                {
                                    meshTextures.Add(mt);
                                }
                            }
                        }
                    } 
                }
            }
            names += $"Scene mesh and texture names for scene {scene.name}:\r\n\r\n\t"+ string.Join("\r\n\t", meshTextures.ToArray());
            Debug.Log(names);
        }
    }
}
