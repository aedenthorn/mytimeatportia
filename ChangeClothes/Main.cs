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
using Pathea.AppearNs;
using Pathea.FavorSystemNs;
using Pathea.ItemSystem;
using Pathea.Maths;
using Pathea.MessageSystem;
using Pathea.ModuleNs;
using Pathea.NpcAppearNs;
using Pathea.NpcRepositoryNs;
using Pathea.ScenarioNs;
using UnityEngine;
using UnityEngine.SceneManagement;
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
                UnityEngine.Debug.Log((pref ? "ClothesChanger " : "") + str);
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

            //MessageManager.Instance.Subscribe("WakeUpScreen", new Action<object[]>(ChangeAllClothes));
            MessageManager.Instance.Subscribe("WakeUpScreenEnd", new Action<object[]>(ChangeAllClothes));

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

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

        private static void ChangeAllClothes(object[] obj = null)
        {
            if (!enabled)
                return;

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

        [HarmonyPatch(typeof(NpcAppear), "InitAppear")]
        static class InitAppear_Patch
        {
            static void Postfix(NpcAppearDBData npcAppearDBData, NpcAppearData appearData, Actor actor)
            {
                if (!enabled)
                    return;

                for (int i = 0; i < clothesChangers.Count; i++)
                {
                    Dbgl($"changing for {i} {clothesChangers[i].name}");
                    if (clothesChangers[i].id == actor.InstanceId)
                    {
                        Dbgl($"actor: {actor.ActorName}");
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
    }
}
