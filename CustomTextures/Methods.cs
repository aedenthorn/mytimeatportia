using Harmony12;
using Pathea;
using Pathea.ActorNs;
using Pathea.Expression;
using Pathea.HomeNs;
using Pathea.ModuleNs;
using Pathea.NpcAppearNs;
using Pathea.RiderNs;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomTextures
{
    public partial class Main
    {

        private static Dictionary<int, Texture2D> customTextures = new Dictionary<int, Texture2D>();
        private static Dictionary<int, Dictionary<int, Texture2D>> customTexturesPartial = new Dictionary<int, Dictionary<int, Texture2D>>();
        private static Dictionary<int, Dictionary<string, Texture2D>> customTexturesExact = new Dictionary<int, Dictionary<string, Texture2D>>();
        private static Dictionary<string, Texture2D> customTexturesMisc = new Dictionary<string, Texture2D>();
        private static Dictionary<string, Texture2D> customTexturesHorse = new Dictionary<string, Texture2D>();

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
            Regex pattern4 = new Regex(@"^Horse_..*\.png$");

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
                    customTextures[id] = tex;
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
                else if (pattern4.IsMatch(fileName))
                {
                    Dbgl($"got horse name texture at path: {file}");
                    string name = fileName.Substring(6, fileName.Length - 10);
                    Dbgl($"got horse name: '{name}'");
                    Texture2D tex = new Texture2D(2, 2);
                    byte[] imageData = File.ReadAllBytes(file);
                    tex.LoadImage(imageData);
                    customTexturesHorse[name] = tex;
                }
                else
                {
                    Dbgl($"got misc texture at path: {file}");
                    Texture2D tex = new Texture2D(2, 2);
                    byte[] imageData = File.ReadAllBytes(file);
                    tex.LoadImage(imageData);
                    customTexturesMisc[fileName.Substring(0, fileName.Length - 4)] = tex;
                }
            }
        }

        private static void ReloadTextures()
        {
            LoadCustomTextures();
            FixSceneTextures(SceneManager.GetActiveScene());
            ReloadActorTextures();
        }
        private static void ReloadActorTextures()
        {
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
                    SkinnedMeshRenderer[] smrs = actor.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                    for(int i = 0; i < smrs.Length; i++)
                    {
                        if (smrs[i].material?.HasProperty("_MainTex") == true && smrs[i].material.mainTexture != null)
                        {
                            smrs[i].material.mainTexture = kvp.Value;
                        }
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


            int[] rint = Module<RidableModuleManager>.Self.GetAllRidableUid();

            foreach(int r in rint)
            {
                IRidable ridable = Module<RidableModuleManager>.Self.GetRidable(r);
                if (ridable == null)
                    continue;
                string name = ridable.GetNickName();
                Dbgl($"got horse '{name}'");
                if (customTexturesHorse.ContainsKey(name))
                {
                    Dbgl($"got horse texture for {name}");
                    GameObject go = ridable.GetActor().gameObject;
                    SkinnedMeshRenderer[] smrs = go.GetComponentsInChildren<SkinnedMeshRenderer>();
                    foreach (SkinnedMeshRenderer mr in smrs)
                    {
                        if (mr.material?.HasProperty("_MainTex") == true && mr.material.mainTexture != null)
                        {
                            Dbgl($"Changing smr texture for {mr.name}");
                            if (mr.name == "saddle")
                            {
                                Dbgl($"Changing saddle");
                                if (customTexturesMisc.ContainsKey($"Saddle_{name}"))
                                {
                                    Texture2D tex = customTexturesMisc[$"Saddle_{name}"];
                                    tex.name = $"Saddle_{name}.png";
                                    mr.material.mainTexture = tex;
                                }
                            }
                            else
                            {
                                Texture2D tex = customTexturesHorse[name];
                                tex.name = $"Horse_{name}.png";
                                mr.material.mainTexture = tex;
                            }
                        }
                    }
                }
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

            if (Module<Player>.Self != null && Module<Player>.Self.actor != null)
            {
                FixOneTexture(Module<Player>.Self.actor.gameObject);

                BlinkController bc = (BlinkController) typeof(Actor).GetField("blinkController", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Module<Player>.Self.actor);
                if(bc != null)
                {
                    SkinnedMeshRenderer mr = (SkinnedMeshRenderer)typeof(BlinkController).GetField("_mesh", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(bc);
                    if(mr != null)
                    {
                        Dbgl($"Blink controller name: {mr.name} - use 'Blink_{mr.name}'");
                        string name = $"Blink_{mr.name}";
                        if (customTexturesMisc.ContainsKey(name) && mr.material && mr.material.HasProperty("_MainTex") && mr.material.mainTexture != null)
                        {
                            Dbgl($"Changing _MainTex texture for {name}");
                            Texture2D tex = customTexturesMisc[name];
                            tex.name = $"{name}.png";
                            mr.material.mainTexture = tex;
                        }
                        if (customTexturesMisc.ContainsKey($"{name}_Brightness") && mr.material && mr.material.HasProperty("_Brightness") && mr.material.mainTexture != null)
                        {
                            Dbgl($"Changing _Brightness texture for {name}");
                            Texture2D tex = customTexturesMisc[$"{name}_Brightness"];
                            tex.name = $"{name}_Brightness.png";
                            mr.material.SetTexture($"{name}_Brightness", tex);
                        }
                        if (customTexturesMisc.ContainsKey($"{name}_Mask") && mr.material && mr.material.HasProperty("_Mask") && mr.material.mainTexture != null)
                        {
                            Dbgl($"Changing _Mask texture for {name}");
                            Texture2D tex = customTexturesMisc[$"{name}_Mask"];
                            tex.name = $"{name}_Mask.png";
                            mr.material.SetTexture($"{name}_Mask", tex);
                        }
                    }
                }
            }

            if (arg1.name == "Main")
            {
                FixWorkshopTextures();
            }
            else if (arg1.name == "PlayerHome")
            {
                FixHomeTextures();
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
                    Texture2D tex = customTexturesMisc[name];
                    tex.name = $"{name}.png";
                    mr.material.mainTexture = tex;
                }
            }
            SkinnedMeshRenderer[] smrs = go.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer mr in smrs)
            {
                string name = mr.name;
                if (customTexturesMisc.ContainsKey(name) && mr.material && mr.material.HasProperty("_MainTex") && mr.material.mainTexture != null)
                {
                    Dbgl($"Changing texture for {name}");
                    Texture2D tex = customTexturesMisc[name];
                    tex.name = $"{name}.png";
                    mr.material.mainTexture = tex;
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
            string names = "";
            List<string> meshTextures = new List<string>();

            if (Module<Player>.Self != null && Module<Player>.Self.actor != null)
            {
                GameObject player = Module<Player>.Self.actor.gameObject;
                names += $"Player actor mesh and texture names:\r\n\r\n\t";


                MeshRenderer[] mrs = player.GetComponentsInChildren<MeshRenderer>();

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

                SkinnedMeshRenderer[] smrs = player.GetComponentsInChildren<SkinnedMeshRenderer>();

                foreach (SkinnedMeshRenderer mr in smrs)
                {
                    if (mr.material && mr.material.HasProperty("_MainTex") && mr.material.mainTexture != null)
                    {
                        string mt = $"smesh name: {mr.name} texture name: {mr.material.mainTexture.name}";
                        if (!meshTextures.Contains(mt))
                        {
                            meshTextures.Add(mt);
                        }

                    }
                }

                BlinkController bc = (BlinkController)typeof(Actor).GetField("blinkController", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Module<Player>.Self.actor);
                if (bc != null)
                {
                    SkinnedMeshRenderer smr = (SkinnedMeshRenderer)typeof(BlinkController).GetField("_mesh", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(bc);
                    if (smr != null && smr.material && smr.material.HasProperty("_MainTex") && smr.material.mainTexture != null)
                    {
                        string mt = $"blink mesh name: {smr.name} texture name: {smr.material.mainTexture.name}";
                        if (!meshTextures.Contains(mt))
                        {
                            meshTextures.Add(mt);
                        }
                    }
                }

                names += string.Join("\r\n\t", meshTextures.ToArray()) + "\r\n\r\n";
                meshTextures.Clear();
            }

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
