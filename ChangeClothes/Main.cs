using BehaviorDesigner.Runtime.Tasks.Basic.UnityGameObject;
using Harmony12;
using Pathea;
using Pathea.ActorNs;
using Pathea.AppearNs;
using Pathea.ItemSystem;
using Pathea.MessageSystem;
using Pathea.ModuleNs;
using Pathea.NpcAppearNs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;
using static Harmony12.AccessTools;

namespace ChangeClothes
{
    public static partial class Main
    {
        private static bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
            {
                Debug.Log((pref ? "ClothesChanger " : "") + str);
                return;
                using (StreamWriter w = File.AppendText(@"G:\ga\MTAP\log.txt")) 
                { 
                    w.WriteLine(str);
                }

                //File.AppendAllText(@"g:\ga\MTAP\mm\log.txt", str + Environment.NewLine);
            }
        }

        public static bool enabled;
        private static List<int> hairs = new List<int>()
        {
            10,
            13,
            20,
            23,
            30,
            32,
            80,
            82,
            70,
            72,
            60,
            62,
            50,
            52,
            40,
            42
        };
        private static List<string> hairNames = new List<string>()
        {
            "Arlo_Hair_001",
            "Arlo_DLCHair_001",
            "Drxu_Hair_001",
            "Drxu_DLCHair_001",
            "Emily_Hair_001",
            "Emily_DLCHair_001",
            "Ginger_Hair_001",
            "Ginger_DLCHair_001",
            "Gust_Hair_001",
            "Gust_DLCHair_001",
            "Mint_Hair_001",
            "Mint_DLCHair_001",
            "Phyllis_Hair_001",
            "Phyllis_DLCHair_001",
            "Sam_Hair_001",
            "Sam_DLCHair_001"
        };
        private static List<int> clothes = new List<int>()
        {
            11,
            15,
            21,
            25,
            31,
            34,
            81,
            84,
            71,
            74,
            61,
            64,
            51,
            54,
            41,
            44
        };
        private static List<string> clothesNames = new List<string>()
        {
            "Arlo_Clothes_001",
            "Arlo_DLCClothes_001",
            "Drxu_Clothes_001",
            "Drxu_DLCClothes_001",
            "Emily_Clothes_001",
            "Emily_DLCClothes_001",
            "Ginger_Clothes_001",
            "Ginger_DLCClothes_001",
            "Gust_Clothes_001",
            "Gust_DLCClothes_001",
            "Mint_Clothes_001",
            "Mint_DLCClothes_001",
            "Phyllis_Clothes_001",
            "Phyllis_DLCClothes_001",
            "Sam_Clothes_001",
            "Sam_DLCClothes_001"
        };
        private static List<string> playerHairNames = new List<string>();
        private static List<int> playerHair = new List<int>();
        private static List<string> playerClothesNames = new List<string>();
        private static List<int> playerClothes = new List<int>();
        private static Dictionary<string, AppearUnit> playerAppearUnits = new Dictionary<string, AppearUnit>();

        private static List<ClothesChanger> clothesChangers = new List<ClothesChanger>()
        {
            //new ClothesChanger("Alice",4000050),
            new ClothesChanger("Arlo",4000063),
            new ClothesChanger("Dr. Xu",4000092),
            new ClothesChanger("Emily",4000003),
            new ClothesChanger("Ginger",4000093),
            new ClothesChanger("Gust",4000091),
            new ClothesChanger("Mint",4000111),
            new ClothesChanger("Phyllis",4000035),
            new ClothesChanger("Sam",4000067)
        };

        private static List<ClothesChanger> noClothes = new List<ClothesChanger>()
        {
            //new ClothesChanger("Alice",4000050),
        };

        public static Settings settings { get; private set; }

        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnHideGUI = OnCloseGUI;

            playerClothes = new List<int>(clothes);
            playerClothes.Insert(0, 0);
            playerClothesNames = new List<string>(clothesNames);
            playerClothesNames.Insert(0, "default");
            playerHair = hairs;
            playerHair.Insert(0, 0);
            playerHairNames = hairNames;
            playerHairNames.Insert(0, "default");
            Dbgl("loaded arrays");

            //MessageManager.Instance.Subscribe("WakeUpScreen", new Action<object[]>(ChangeAllClothes));
            MessageManager.Instance.Subscribe("WakeUpScreenEnd", new Action<object[]>(ChangeAllClothes));

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            wasPlayerEnabled = settings.EnablePlayerModelling;

        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }
        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            ChangeAllClothes();
            settings.Save(modEntry);
        }
        private static void OnCloseGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
            ChangeAllClothes();

        }

        private static bool wasPlayerEnabled = false;
        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (settings.EnablePlayerModelling != wasPlayerEnabled && Module<Player>.Self.actor != null)
            {
                Dbgl("loading cloth");
                Module<Player>.Self.actor.ApplyCloth(false);
                wasPlayerEnabled = settings.EnablePlayerModelling;
            }
            settings.EnablePlayerModelling = GUILayout.Toggle(settings.EnablePlayerModelling, " Enable custom clothes for player", new GUILayoutOption[0]);
            if (settings.EnablePlayerModelling)
            {
                GUILayout.Label(string.Format("Player clothes: <b>{0}</b>", playerClothesNames[settings.playerClothes]), new GUILayoutOption[0]);
                settings.playerClothes = (int)GUILayout.HorizontalSlider(settings.playerClothes, 0f, playerClothes.Count - 1, new GUILayoutOption[0]);
                GUILayout.Space(10f);
                GUILayout.Label(string.Format("Player hair: <b>{0}</b>", playerHairNames[settings.playerHair]), new GUILayoutOption[0]);
                settings.playerHair = (int)GUILayout.HorizontalSlider(settings.playerHair, 0f, playerHairNames.Count - 1, new GUILayoutOption[0]);
                GUILayout.Space(10f);
                GUILayout.Label(string.Format("Player head offset: <b>{0:F2}</b>", settings.playerHeadOffset), new GUILayoutOption[0]);
                settings.playerHeadOffset = GUILayout.HorizontalSlider((settings.playerHeadOffset+0.25f)*100, 0f, 50, new GUILayoutOption[0])/100f - 0.25f;
            }
            //GUILayout.Label(string.Format("Hair: <b>{0}</b>", hairNames[settings.PlayerHair]), new GUILayoutOption[0]);
            //settings.PlayerHair = (int)GUILayout.HorizontalSlider(settings.PlayerHair, 0f, hairs.Count - 1, new GUILayoutOption[0]);
            GUILayout.Space(20f);
            for (int i = 0; i < clothesChangers.Count; i++)
            {
                GUILayout.Label($"<b>{clothesChangers[i].name}</b>", new GUILayoutOption[0]);
                GUILayout.Label(string.Format("Hair: <b>{0}</b>", hairNames[settings.hairs[i]]), new GUILayoutOption[0]);
                settings.hairs[i] = (int)GUILayout.HorizontalSlider(settings.hairs[i], 0f, hairs.Count-1, new GUILayoutOption[0]);
                GUILayout.Space(10f);
                GUILayout.Label(string.Format("Clothes: <b>{0}</b>", clothesNames[settings.clothes[i]]), new GUILayoutOption[0]);
                settings.clothes[i] = (int)GUILayout.HorizontalSlider(settings.clothes[i], 0f, clothes.Count-1, new GUILayoutOption[0]);
                GUILayout.Space(20f);
            }
        }

        private static void ChangeAllClothes(object[] obj = null)
        {
            if (!enabled)
                return;

            if(Module<Player>.Self.actor != null)
            {
                Module<Player>.Self.actor.ApplyCloth(false);
            }


            /*
            if (settings.EnablePlayerModelling)
            {
                Dbgl("player");

                //AccessTools.FieldRefAccess<Player, SkinnedMeshRenderer>(Singleton<Player>.Instance, "skinnedMeshRenderer");
                DynamicBone[] dybones = Module<Player>.Self.actor.Equip.GetComponentsInChildren<DynamicBone>(true);
                Transform[] bones = Module<Player>.Self.actor.Equip.GetComponentsInChildren<Transform>(true);
                for (int j = 0; j < dybones.Length; j++)
                {
                    //Dbgl("dybones");
                    Transform transform = dybones[j].transform;
                    transform.position += new Vector3(0f, j/10f, 0f);
                    Dbgl(transform.name + " " + transform.position);
                }
                for (int j = 0; j < bones.Length; j++)
                {
                    Transform transform = bones[j];
                    bones[j].position += new Vector3(0f, j/10f, 0f);
                    Dbgl(transform.name + " " + transform.position);
                }
                //Module<Player>.Self.actor.RefreshMeshReference(smr);
                //Module<Player>.Self.actor.ApplyCloth(false);
            }
            */

            //ChangeClothes(Player.Self.actor, hairs[settings.PlayerHair], -1);
            for (int i = 0; i < clothesChangers.Count; i++)
            {
                Actor actor = Module<ActorMgr>.Self.Get(clothesChangers[i].id);
                if (actor == null)
                {
                    continue;
                }
                ChangeClothes(actor, hairs[settings.hairs[i]], clothes[settings.clothes[i]]);
            }
        }


        private static void ChangeClothes(Actor actor, int hairId, int clothesId)
        {
            if (!enabled)
                return;

            Dictionary<int, AppearDBData> appearDbDatas = AccessTools.FieldRefAccess<NpcAppearModule, Dictionary<int, AppearDBData>>(NpcAppearModule.Self,"m_AppearDBDataDic");
            
            Dbgl($"changing clothes for {actor.ActorName}");


            NpcAppear component = actor.GetComponent<NpcAppear>();
            if (component == null)
            {
                Dbgl($"no npcappear for {actor.ActorName}");
                return;
            }
            if (hairId > -1)
                component.SetPart(appearDbDatas[hairId]);
            if(clothesId > -1)
                component.SetPart(appearDbDatas[clothesId]);
            /*
            SkinnedMeshRenderer[] smrs = AccessTools.FieldRefAccess<NpcAppear, SkinnedMeshRenderer[]>(component,
                "m_Smrs");

            SkinnedMeshRenderer[] mSmrs = new SkinnedMeshRenderer[smrs.Length];
            for (int j = 0; j < mSmrs.Length; j++)
            {
                Texture2D tex = (Texture2D)smrs[j].material.mainTexture;
                Color[] data = tex.GetPixels();
                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i] != Color.clear && ((data[i].b > data[i].r && data[i].g > data[i].r) || data[i].r - data[i].b > 200 && data[i].r - data[i].g > 200))
                    {
                        data[i] = new Color(data[i].b, data[i].r, data[i].g, 255);
                    }
                }
                Texture2D outTex = new Texture2D(tex.width, tex.height);
                outTex.SetPixels(data);
                smrs[j].material.mainTexture = outTex;
                mSmrs[j] = smrs[j];
            }

            AccessTools.FieldRefAccess<NpcAppear, SkinnedMeshRenderer[]>(component,
                "m_Smrs") = mSmrs;
            */
        }
        internal class ClothesChanger
        {

            public ClothesChanger(string _name, int _id)
            {
                this.name = _name;
                this.id = _id;
            }

            public string name { get; }
            public int id { get; }
        }


        [HarmonyPatch(typeof(AppearTarget), "BuildMesh", new Type[] { typeof(List<AppearUnit>), typeof(AppearData), typeof(AppearUnit), typeof(string) })]
        static class BuildMesh1_Patch
        {
            static void Prefix(AppearTarget __instance, AppearData appearData, ref List<AppearUnit> units)
            {
                Dbgl($"Building player mesh");

                if (!enabled || !settings.EnablePlayerModelling)
                    return;
                /*
                List<string> bones = new List<string>();
                Dictionary<string, Transform> bonedic = AccessTools.FieldRefAccess<AppearTarget, Dictionary<string, Transform>>(__instance, "targetBones");
                List<string> keys = new List<string>(bonedic.Keys);
                for(int i = 0; i < keys.Count; i++)
                {
                    Transform bone = bonedic[keys[i]];
                    if(bone.name == "Linda_Model")
                    {
                        isFemale = true;
                        break;
                    }
                    //bonedic[keys[i]].position += new Vector3(i/10f, i/10f, 0);
                    //bones.Add(bone.name + " "+bone.position);
                }
                */
                //Dbgl(string.Join("\r\n", bones.ToArray()));
                
                for (int i = 0; i < units.Count; i++)
                {
                    //Dbgl(units[i].name + " " + units[i].Part + " "+ (units[i].Smr != null && units[i].Smr.rootBone != null? units[i].Smr.rootBone.position+"" : ""));
                    bool isFemale = units[i].name.StartsWith("AppearUnit_Linda");

                    if (units[i].Part == AppearPartEnum.Body && settings.playerClothes > 0)
                    {
                        if (!playerAppearUnits.ContainsKey(units[i].name))
                        { 
                            playerAppearUnits.Add(units[i].name, GameObject.Instantiate<AppearUnit>(units[i]));
                        }
                        units[i] = playerAppearUnits[units[i].name];
                        units[i].Smr = Singleton<ResMgr>.Instance.LoadSyncByType<NpcAppearUnit>(AssetType.NpcAppear, playerClothesNames[settings.playerClothes]).smrs[0];
                        for (int j = 0; j < units[i].Smr.bones.Length; j++)
                        {
                            
                            if (units[i].Smr.bones[j].name == "Bip001 Spine1")
                            {
                                if (isFemale)
                                {
                                    Transform bone = Transform.Instantiate(units[i].Smr.bones[j], units[i].Smr.bones[j].parent, false);
                                    units[i].Smr.bones[j] = bone;
                                    units[i].Smr.bones[j].name = "Bip001 Spine2";

                                    int count = units[i].Smr.bones[j].childCount;
                                    for (int k = 0; k < count; k++)
                                    {
                                        Transform child = units[i].Smr.bones[j].GetChild(k);
                                        //Dbgl(child.name + " "+child.position);
                                    }
                                    //units[i].Smr.bones[j].localPosition += new Vector3(0,2f,0);
                                }
                            }
                        }

                        /*
                        Matrix4x4[] bindPoses;
                        bindPoses = new Matrix4x4[units[i].Smr.sharedMesh.bindposes.Length + 1];
                        Transform[] newBones = new Transform[units[i].Smr.bones.Length+1];
                        Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();
                        foreach (Transform bone in units[i].Smr.bones) boneMap[bone.gameObject.name] = bone;
                        Dbgl("2");units[i].Smr.bones[j]
                        
                        Mesh mesh2 = (Mesh)typeof(AppearTarget).GetMethod("BakeToMesh", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { units[i], appearData });
                        List<Transform> tmpMeshRefBones = new List<Transform>();
                        for (int k = 0; k < mesh2.subMeshCount; k++)
                        {
                            CombineInstance item = default(CombineInstance);
                            item.mesh = mesh2;
                            item.subMeshIndex = k;
                            typeof(AppearTarget).GetMethod("FindBonesByName", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { units[i].Smr.bones, tmpMeshRefBones });
                        }
                        List<string> bones = new List<string>();
                        for (int k = 0; k < tmpMeshRefBones.Count; k++)
                        {
                            bones.Add(tmpMeshRefBones[k].name + " " + tmpMeshRefBones[k].position + " " + tmpMeshRefBones[k].localPosition);
                            if (tmpMeshRefBones[k].name.Contains("Spine1"))
                            {
                                tmpMeshRefBones[k].position += new Vector3(0, 1f, 0);
                                //boneA[j].position += new Vector3(2f,0,0);
                                //boneA[j].localPosition += new Vector3(0,2f,0);

                            }
                        }
                        Dbgl(string.Join("\r\n", bones.ToArray()));

                        __state = units[i].Smr.bones;
                        */
                    }
                    if (units[i].Part == AppearPartEnum.Foot && settings.playerClothes > 0)
                    {
                        if (settings.playerHair > 0)
                        {
                            //units[i] = GameObject.Instantiate<AppearUnit>(units[i], units[i].);
                            units[i].Smr = null;
                        }
                    }
                    else if (units[i].Part == AppearPartEnum.Head && units[i].Smr != null && units[i].Smr.bones != null)
                    {
                        if (isFemale)
                        {
                            units[i].Smr.rootBone.position = new Vector3(0, 1.13f+settings.playerHeadOffset, 0);
                        }
                        else
                        {
                            units[i].Smr.rootBone.position = new Vector3(0, 1f + settings.playerHeadOffset, 0);
                        }

                    }
                    else if (units[i].Part == AppearPartEnum.Hair && units[i].Smr != null && units[i].Smr.bones != null)
                    {
                        if(settings.playerHair > 0) 
                        {
                            if (!playerAppearUnits.ContainsKey(units[i].name))
                            { 
                                playerAppearUnits.Add(units[i].name, GameObject.Instantiate<AppearUnit>(units[i]));
                            }
                            units[i] = playerAppearUnits[units[i].name];
                            units[i].Smr = Singleton<ResMgr>.Instance.LoadSyncByType<NpcAppearUnit>(AssetType.NpcAppear, playerHairNames[settings.playerHair]).smrs[0];
                        }
                        if (isFemale)
                        {
                            units[i].Smr.rootBone.position = new Vector3(0, 1.35f + settings.playerHeadOffset, 0);
                        }
                        else
                        {
                            units[i].Smr.rootBone.position = new Vector3(0, 1.40f + settings.playerHeadOffset, 0);
                        }
                    }
                }

            }
            static void Postfix(AppearTarget __instance, ref SkinnedMeshRenderer __result)
            {
                if (!enabled || !settings.EnablePlayerModelling)
                    return;

                List<string> bones = new List<string>();
                for (int j = 0; j < __result.bones.Length; j++)
                {
                    if (__result.bones[j].name == "Bip001 Spine2")
                    {
                        //Dbgl(__result.bones[j].name + " " + __result.bones[j].position);
                    }
                }
                //Dbgl(string.Join("\r\n", bones.ToArray()));
            }
        }
        //[HarmonyPatch(typeof(ActorEquip), "ApplyCloth")]
        static class ApplyCloth_Patch
        {
            static void Postfix(GameObject ___clothRoot)
            {
                if (!enabled || !settings.EnablePlayerModelling)
                    return;

                Transform[] boneA = ___clothRoot.GetComponentsInChildren<Transform>();

                List<string> bones = new List<string>();
                for (int j = 0; j < boneA.Length; j++)
                {
                    bones.Add(boneA[j].name + " " + boneA[j].position + " " + boneA[j].localPosition);
                    if (boneA[j].name.Contains("Spine"))
                    {
                        //boneA[j].position += new Vector3(0, 0, j / 2f);
                        //boneA[j].position += new Vector3(2f,0,0);
                        //boneA[j].localPosition += new Vector3(0,2f,0);

                    }
                }
                Dbgl(string.Join("\r\n", bones.ToArray()));
            }
        }


        [HarmonyPatch(typeof(NpcAppear), "InitAppear")]
        static class InitAppear_Patch
        {
            static void Postfix(NpcAppearDBData npcAppearDBData, NpcAppearData appearData, Actor actor)
            {
                if (!enabled)
                    return;

                for (int i = 0; i < clothesChangers.Count; i++)
                {
                    //Dbgl($"changing for {i} {clothesChangers[i].name}");
                    if (clothesChangers[i].id == actor.InstanceId)
                    {
                        //Dbgl($"actor: {actor.ActorName}");
                        Dictionary<int, AppearDBData> appearDbDatas = AccessTools.FieldRefAccess<NpcAppearModule, Dictionary<int, AppearDBData>>(Module<NpcAppearModule>.Self, "m_AppearDBDataDic");
                        NpcAppear component = actor.GetComponent<NpcAppear>();
                        if (component == null)
                        {
                            Dbgl("actor does not have npcappear");
                            return;
                        }
                        component.SetPart(appearDbDatas[hairs[settings.hairs[i]]]);
                        component.SetPart(appearDbDatas[clothes[settings.clothes[i]]]);
                    }
                }
            }
        }

        //[HarmonyPatch(typeof(Actor), "RefreshMeshReference")]
        static class RefreshMeshReference_Patch
        {
            static void Prefix(Actor __instance, SkinnedMeshRenderer meshRenderer)
            {
                if (!enabled)
                    return;

                if (__instance.ActorName != "Phyllis")
                    return;

                List<string> bones = new List<string>();
                for (int j = 0; j < meshRenderer.bones.Length; j++)
                {
                    bones.Add(meshRenderer.bones[j].name + " " + meshRenderer.bones[j].position);
                }
                //Dbgl("\r\nPhyllis bones: \r\n"+string.Join("\r\n", bones.ToArray()));

            }
        }
        //[HarmonyPatch(typeof(NpcAppear), "RebuildMesh", new Type[] { typeof(List<NpcAppearUnit>), typeof(Transform), typeof(SkinnedMeshRenderer[]) })]
        static class RebuildMesh2_Patch
        {
            static void Prefix(ref List<NpcAppearUnit> npcAppearUnits)
            {
                if (!enabled)
                    return;
            }
        }

        //[HarmonyPatch(typeof(Actor), "Start")]
        private static class Actor_Start_Patch
        {
            private static void Postfix(ref Actor __instance, ref SkinnedMeshRenderer ___skinnedMeshRenderer)
            {
                FieldRef<Actor, SkinnedMeshRenderer> rendererByRef = FieldRefAccess<Actor, SkinnedMeshRenderer>("skinnedMeshRenderer");

                if (__instance.InstanceId == 4000050)
                {
                    if (___skinnedMeshRenderer != null)
                    {
                        ___skinnedMeshRenderer.sharedMesh = rendererByRef(ActorMgr.Self.Get(4000011)).sharedMesh;
                    }
                }
            }
        }
        //[HarmonyPatch(typeof(ActorInfo), "Instantiate")]
        private static class ActorInfo_Instantiate_Patch
        {
            private static void Prefix(ref ActorInfo __instance, ref string ___model)
            {
                if(___model == "Actor/Npc_Alice")
                {
                    ___model = "Actor/Npc_Ursula";
                }
            }
        }

        //[HarmonyPatch(typeof(NpcAppearModule), "AfterPlayerAwake")]
        static class NpcAppearModule_AfterPlayerAwake_Patch
        {
            static void Prefix(NpcAppearModule __instance, ref Dictionary<int, NpcAppearDBData> ___m_NpcAppearDBDataDir)
            {
                return;
                if (!enabled)
                    return;

                NpcAppearDBData phyllisData = ___m_NpcAppearDBDataDir[4000035];
                NpcAppear phyllisAppear = ActorMgr.Self.Get(4000035).GetComponent<NpcAppear>();

                foreach (ClothesChanger cc in noClothes)
                {
                    continue;
                    ___m_NpcAppearDBDataDir.Add(cc.id, phyllisData);
                    Actor actor = ActorMgr.Self.Get(cc.id);
                    actor.gameObject.AddComponent<NpcAppear>();
                    NpcAppear component = actor.GetComponent<NpcAppear>();
                    AccessTools.FieldRefAccess<NpcAppear, Dictionary<int, AppearDBData>>(component, "m_DefaultAppearDBDatas") = AccessTools.FieldRefAccess<NpcAppear, Dictionary<int, AppearDBData>>(phyllisAppear, "m_DefaultAppearDBDatas");
                    AccessTools.FieldRefAccess<NpcAppear, int[]>(component, "defaultAppearIDs") = AccessTools.FieldRefAccess<NpcAppear, int[]>(phyllisAppear, "defaultAppearIDs");
                    AccessTools.FieldRefAccess<NpcAppear, Transform>(component, "m_BoneRoot") = AccessTools.FieldRefAccess<NpcAppear, Transform>(phyllisAppear, "m_BoneRoot");
                    AccessTools.FieldRefAccess<NpcAppear, SkinnedMeshRenderer[]>(component, "m_Smrs") = AccessTools.FieldRefAccess<NpcAppear, SkinnedMeshRenderer[]>(phyllisAppear, "m_Smrs");
                    //component.InitAppear(phyllisData, phyllisAppear.appearData, actor);
                    typeof(NpcAppearModule).GetMethod("InitNPCAppear", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { cc.id, phyllisAppear.appearData, phyllisAppear.npcAppearDBData });
                    //typeof(NpcAppear).GetProperty("appearData").SetValue(component, phyllisAppear.appearData, null);
                    //AccessTools.FieldRefAccess<NpcAppear, Dictionary<int, AppearDBData>>(component, "m_DefaultAppearDBDatas") = AccessTools.FieldRefAccess<NpcAppear, Dictionary<int, AppearDBData>>(phyllisAppear, "m_DefaultAppearDBDatas");
                    //AccessTools.FieldRefAccess<NpcAppear, Actor>(component, "m_Actor") = actor;
                }
            }
        }
        //[HarmonyPatch(typeof(NpcAppear), "SetAppearPart")]
        static class SetAppearPart_Patch
        {
            static void Prefix(object appearPart)
            {
                if (!enabled)
                    return;

                if (appearPart == null)
                {
                    Dbgl("appearPart is null");
                }
            }
        }
        //[HarmonyPatch(typeof(Player), "ApplySubModel")]
        static class Player_ApplySubModel_Patch
        {
            static void Postfix(ActorEquip.EEquipSlot slot, ItemEquipmentCmpt Equipment)
            {
                Dbgl($"applying submodel to slot {slot}");
                if (!enabled || Equipment == null)
                    return;

                string text = Equipment.SubPathWoman;
                if (text != null)
                {
                    Dbgl($"applying submodel {text}");
                    Actor emily = ActorMgr.Self.Get(4000003);
                    Actor alice = ActorMgr.Self.Get(4000050);

                    typeof(Actor).GetField("subModels",BindingFlags.NonPublic | BindingFlags.Instance).SetValue(emily, typeof(Actor).GetField("subModels", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Module<Player>.Self.actor));
                    typeof(Actor).GetField("subModels",BindingFlags.NonPublic | BindingFlags.Instance).SetValue(alice, typeof(Actor).GetField("subModels", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Module<Player>.Self.actor));


                    ActorMgr.Self.Get(4000035).ApplySubModel((int)slot, Equipment.SubPathWoman);
                    ActorMgr.Self.Get(4000003).ApplySubModel((int)slot, Equipment.SubPathWoman);
                    ActorMgr.Self.Get(4000050).ApplySubModel((int)slot, Equipment.SubPathWoman);
                }

            }
        }
        //[HarmonyPatch(typeof(Player), "UpdateAppear")]
        static class Player_UpdateAppear_Patch
        {
            static void Postfix(Player __instance)
            {
                if (!enabled)
                    return;

                string[] array = new string[5];
                for (int i = 0; i < array.Length; i++)
                {
                    AppearPartEnum part = (AppearPartEnum)i;
                    ActorEquip.EEquipSlot eequipSlot = ActorEquip.AppaerPartEnumToEEquipSlot(part);
                    if (eequipSlot != ActorEquip.EEquipSlot.Max)
                    {
                        array[i] = __instance.GetInfo(__instance.bag.EquipSlot.GetItemEquipmentCmpt(eequipSlot));
                    }
                }

                Dbgl($"updating appearances");

                Actor emily = ActorMgr.Self.Get(4000003);
                Actor alice = ActorMgr.Self.Get(4000050);

                emily.gameObject.AddComponent<ActorEquip>();
                alice.gameObject.AddComponent<ActorEquip>();

                ActorMgr.Self.Get(4000003).SetEquipInfo(array);
                ActorMgr.Self.Get(4000050).SetEquipInfo(array);

                AccessTools.FieldRefAccess<ActorEquip, string[]>(emily.GetComponent<ActorEquip>(), "nudeAppearUnits") = Module<Player>.Self.actor.GetComponent<ActorEquip>().GetCurrentNudeAppearsPath();
                AccessTools.FieldRefAccess<ActorEquip, string[]>(emily.GetComponent<ActorEquip>(), "equipAppearUnits") = Module<Player>.Self.actor.GetComponent<ActorEquip>().GetCurrentNudeAppearsPath();
                AccessTools.FieldRefAccess<ActorEquip, string[]>(alice.GetComponent<ActorEquip>(), "nudeAppearUnits") = Module<Player>.Self.actor.GetComponent<ActorEquip>().GetCurrentNudeAppearsPath();
                AccessTools.FieldRefAccess<ActorEquip, string[]>(alice.GetComponent<ActorEquip>(), "equipAppearUnits") = Module<Player>.Self.actor.GetComponent<ActorEquip>().GetCurrentNudeAppearsPath();

                emily.ApplyCloth(true);
                alice.ApplyCloth(true);
            }
        }

        //[HarmonyPatch(typeof(AppearData), "Default")]
        static class AppearData_Default_Patch
        {
            static void Prefix(ref AppearDataAsset asset)
            {
                if (!enabled)
                    return;

                if(asset == null)
                {
                    asset = AccessTools.FieldRefAccess<ActorEquip, AppearDataAsset>(Module<Player>.Self.actor.GetComponent<ActorEquip>(), "defaultAsset");
                }
            }
        }

    }
}
