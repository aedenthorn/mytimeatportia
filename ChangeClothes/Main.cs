using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ChangeClothes;
using Harmony12;
using Pathea;
using Pathea.ActorNs;
using Pathea.FavorSystemNs;
using Pathea.MessageSystem;
using Pathea.NpcAppearNs;
using Pathea.NpcRepositoryNs;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;

namespace ChangeClothes
{
    public static partial class Main
    {
        private static bool isDebug = false;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                UnityEngine.Debug.Log((pref ? "new ClothesChanger " : "") + str);
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

        private static List<ClothesChanger> clothesChangers = new List<ClothesChanger>()
        {
            new ClothesChanger("Arlo",4000063),
            new ClothesChanger("Dr. Xu",4000092),
            new ClothesChanger("Emily",4000003),
            new ClothesChanger("Ginger",4000093),
            new ClothesChanger("Gust",4000091),
            new ClothesChanger("Mint",4000111),
            new ClothesChanger("Phyllis",4000035),
            new ClothesChanger("Sam",4000067)
        };

        public static Settings settings { get; private set; }

        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnHideGUI = OnCloseGUI;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            MessageManager.Instance.Subscribe("WakeUpScreen", new Action<object[]>(OnWakeUp));
            MessageManager.Instance.Subscribe("WakeUpScreenEnd", new Action<object[]>(OnWakeUp));
            //SceneManager.activeSceneChanged += ChangeScene;


        }

        private static void ChangeScene(Scene oldScene, Scene newScene)
        {
            ChangeAllClothes();
        }

        private static void OnWakeUp(object[] obj)
        {
            //ChangeClothes(4000003,52,51);
            ChangeAllClothes();
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

        private static void ChangeAllClothes()
        {
            //ChangeClothes(Player.Self.actor, hairs[settings.PlayerHair], -1);
            for (int i = 0; i < clothesChangers.Count; i++)
            {
                Actor actor = ActorMgr.Self.Get(clothesChangers[i].id);
                if (actor == null)
                {
                    continue;
                }
                ChangeClothes(actor, hairs[settings.hairs[i]], clothes[settings.clothes[i]]);
            }
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            //GUILayout.Label($"<b>{Player.Self.ActorName}</b>", new GUILayoutOption[0]);
            //GUILayout.Label(string.Format("Hair: <b>{0}</b>", hairNames[settings.PlayerHair]), new GUILayoutOption[0]);
            //settings.PlayerHair = (int)GUILayout.HorizontalSlider(settings.PlayerHair, 0f, hairs.Count - 1, new GUILayoutOption[0]);
            GUILayout.Space(10f);
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

        private static void ChangeClothes(Actor actor, int hairId, int clothesId)
        {
            if (!enabled)
                return;

            Dictionary<int, AppearDBData> appearDbDatas =
                AccessTools.FieldRefAccess<NpcAppearModule, Dictionary<int, AppearDBData>>(NpcAppearModule.Self,
                    "m_AppearDBDataDic");

            NpcAppear component = actor.GetComponent<NpcAppear>();
            if (component == null)
            {
                Dbgl("actor does not have npcappear");
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


        [HarmonyPatch(typeof(NpcAppear), "InitAppear")]
        static class InitAppear_Patch
        {
            static void Postfix(ref NpcAppearDBData npcAppearDBData, ref NpcAppearData appearData, Actor actor)
            {
                if (!enabled)
                    return;

                for (int i = 0; i < clothesChangers.Count; i++)
                {
                    if (clothesChangers[i].id == actor.InstanceId)
                    {
                        Dbgl($"actor: {actor.ActorName}");
                        Dictionary<int, AppearDBData> appearDbDatas =
                            AccessTools.FieldRefAccess<NpcAppearModule, Dictionary<int, AppearDBData>>(NpcAppearModule.Self,
                                "m_AppearDBDataDic");
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
    }
}
