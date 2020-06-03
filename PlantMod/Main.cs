using Harmony12;
using Hont;
using Pathea;
using Pathea.HomeNs;
using Pathea.ItemSystem;
using Pathea.ModuleNs;
using Pathea.StoreNs;
using Pathea.UISystemNs;
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityModManagerNet;

namespace PlantMod
{
    public static partial class Main
    {
        private static Settings settings;
        private static bool enabled;
        private static bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                UnityEngine.Debug.Log((pref ? "PlantMod " : "") + str);
        }
        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

        }


        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label(string.Format("Plant Growth Speed Multiplier: <b>{0:F1}x</b>", settings.plantGrowMult), new GUILayoutOption[0]);
            settings.plantGrowMult = GUILayout.HorizontalSlider(settings.plantGrowMult * 10f, 0f, 10000f, new GUILayoutOption[0]) / 10f;
            GUILayout.Space(10f);
            GUILayout.Label(string.Format("Nutrient Consumption Multiplier: <b>{0:F2}</b>", settings.nutrientConsumeMult), new GUILayoutOption[0]);
            settings.nutrientConsumeMult = GUILayout.HorizontalSlider(settings.nutrientConsumeMult * 100f, 0f, 1000f, new GUILayoutOption[0]) / 100f;
            GUILayout.Space(10f);
            bool ignoreSeasons = GUILayout.Toggle(settings.ignoreSeasons, "Ignore Seasons", new GUILayoutOption[0]);
            if(settings.ignoreSeasons != ignoreSeasons)
            {
                settings.ignoreSeasons = ignoreSeasons;
                SetIgnoreSeasons(ignoreSeasons);
            }
            GUILayout.Space(10f);
        }


        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }

        private static void SetIgnoreSeasons(bool ignore)
        {
            List<SeedItemConfData> seedDataList;
            if (ignore)
            {
                seedDataList = AccessTools.FieldRefAccess<ItemDataMgr, List<SeedItemConfData>>(Module<ItemDataMgr>.Self, "seedDataList");
                for (int i = 0; i < seedDataList.Count; i++)
                {
                    seedDataList[i].growthRate = new int[] { 10, 10, 10, 10 };
                    if(seedDataList[i].fruitGrowthRate.Length == 4)
                    {
                        seedDataList[i].fruitGrowthRate = new int[] { 10, 10, 10, 10 };
                    }
                }
                AccessTools.FieldRefAccess<ItemDataMgr, List<SeedItemConfData>>(Module<ItemDataMgr>.Self, "seedDataList") = seedDataList;
            }
            else
            {
                SqliteDataReader reader = LocalDb.cur.ReadFullTable("Item_seed");
                seedDataList = DbReader.Read<SeedItemConfData>(reader, 20);
            }
            ModifyPlants(ref seedDataList);
            AccessTools.FieldRefAccess<ItemDataMgr, List<SeedItemConfData>>(Module<ItemDataMgr>.Self, "seedDataList") = seedDataList;
        }

        private static void ModifyPlants(ref List<SeedItemConfData> seedDataList)
        {
            for(int i = 0; i < seedDataList.Count; i++)
            {
                switch (seedDataList[i].ID)
                {
                    case 6000019: // lemon
                        seedDataList[i].plantName = 270368;
                        seedDataList[i].dropHarvestId = 119;
                        break;
                }
                seedDataList[i].GenerateInfo();
            }
        }

        private static int productId = 7300;
        private static List<StoreItem> storeItems = new List<StoreItem>
        {
            new StoreItem(6000019,productId++,20,-1,"-1",1)
        };

    }
}
