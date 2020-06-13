using Harmony12;
using Hont.ExMethod.Collection;
using Pathea;
using Pathea.AnimalFarmNs;
using Pathea.HomeNs;
using Pathea.TipsNs;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace FarmAnimals
{
    public class Main
    {
        public static Settings settings { get; private set; }

        public static bool enabled;

        private static readonly bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "FarmAnimals " : "") + str);
        }

        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnToggle = OnToggle;

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
            GUILayout.Label(string.Format("Animal Growth Speed: <b>{0:F1}</b>", settings.AnimalGrowthSpeed), new GUILayoutOption[0]);
            settings.AnimalGrowthSpeed = GUILayout.HorizontalSlider(settings.AnimalGrowthSpeed * 10f, 1f, 1000f, new GUILayoutOption[0]) / 10f;
            GUILayout.Space(10f);
            GUILayout.Label(string.Format("Max Animal Growth: <b>{0:F1}%</b>", settings.MaxAnimalGrowthPercent), new GUILayoutOption[0]);
            settings.MaxAnimalGrowthPercent = GUILayout.HorizontalSlider(settings.MaxAnimalGrowthPercent, 1f, 300f, new GUILayoutOption[0]);
            GUILayout.Space(10f);
            GUILayout.Label(string.Format("Base Pregnancy Chance: <b>{0:F1}%</b>", settings.BasePregnancyChance), new GUILayoutOption[0]);
            settings.BasePregnancyChance = GUILayout.HorizontalSlider(settings.BasePregnancyChance, 1f, 100f, new GUILayoutOption[0]);
            GUILayout.Space(10f);
        }

        [HarmonyPatch(typeof(AnimalData), nameof(AnimalData.GetDataByDataId))]
        static class AnimalData_GedDataByDataId
        {
            static void Prefix(List<AnimalData> ___datas)
            {
                if (!enabled)
                    return;
                if (___datas == null)
                {
                    SqliteDataReader sqliteDataReader = LocalDb.cur.ReadFullTable("AnimalFarm_Animal");
                    if (sqliteDataReader == null)
                    {
                        return;
                    }
                    ___datas = DbReader.Read<AnimalData>(sqliteDataReader, 20);
                }
                for(int i = 0; i < ___datas.Count; i++)
                {
                    typeof(AnimalData).GetProperty("TotalPoint").SetValue(___datas[i],(int)Math.Round(___datas[i].StandardPoint * settings.MaxAnimalGrowthPercent / 100f), null);
                    string name = TextMgr.GetStr(___datas[i].NameId, -1);
                    Dbgl($"{name} max points set to {___datas[i].TotalPoint}");
                }
            }
        }

        [HarmonyPatch(typeof(AnimalinFarm), "AddAgeChange")]
        static class AnimalinFarm_AddAgeChange
        {
            static void Prefix(ref float foodNum)
            {
                if (!enabled)
                    return;
                foodNum *= settings.AnimalGrowthSpeed;
            }

            [HarmonyPatch(typeof(AnimalFarmUnit), "Reproduction")]
            static class AnimalFarmUnit_Reproduction
            {
                private static bool Prefix(ref AnimalFarmUnit __instance)
                {
                    if (!enabled)
                        return false;
                    if (__instance.Animals.Count >= __instance.HouseData.AnimalMax)
                    {
                        Dbgl($"farm already full, cancelling");
                        return false;
                    }

                    int tipsId;

                    Dictionary<int, List<AnimalinFarm>> aifs = new Dictionary<int, List<AnimalinFarm>>();
                    foreach(AnimalinFarm aif in __instance.Animals)
                    {
                        if (aif.Age >= aif.Data.StandardPoint)
                        {
                            if (aifs.ContainsKey(aif.Data.Id))
                            {
                                aifs[aif.Data.Id].Add(aif);
                            }
                            else
                            {
                                aifs.Add(aif.Data.Id, new List<AnimalinFarm>() { aif });
                            }
                        }
                    }

                    Dictionary<int, int> newAnimals = new Dictionary<int, int>();
                    foreach(KeyValuePair<int,List<AnimalinFarm>> kvp in aifs)
                    {
                        newAnimals.Add(kvp.Key, 0);
                    }

                    while(aifs.Keys.Count > 0)
                    {
                        if (__instance.Animals.Count >= __instance.HouseData.AnimalMax)
                        {
                            Dbgl($"farm filled up, breaking");
                            break;
                        }
                        int idx = aifs.Keys.ToArray()[UnityEngine.Random.Range(0, aifs.Count - 1)];
                        if(aifs[idx].Count < 2)
                        {
                            aifs.Remove(idx);
                        }
                        else
                        {
                            List<AnimalinFarm> aifl = aifs[idx];
                            List<AnimalinFarm> matureAnimals = aifl.FindAll((AnimalinFarm a) => a.Age >= a.Data.StandardPoint);
                            if(matureAnimals.Count < 2)
                            {
                                aifs.Remove(idx);
                            }
                            else
                            {
                                int a = UnityEngine.Random.Range(0, matureAnimals.Count-1);
                                AnimalinFarm aa = matureAnimals[a];
                                matureAnimals.RemoveAt(a);
                                int b = UnityEngine.Random.Range(0, matureAnimals.Count-1);
                                AnimalinFarm ab = matureAnimals[a];
                                matureAnimals.RemoveAt(b);
                                float chancea = settings.BasePregnancyChance + (100 - settings.BasePregnancyChance) * ((aa.Age - aa.Data.StandardPoint) / aa.Data.StandardPoint);
                                float chanceb = settings.BasePregnancyChance + (100 - settings.BasePregnancyChance) * ((ab.Age - ab.Data.StandardPoint) / ab.Data.StandardPoint);
                                float chance = (chancea + chanceb) / 2f;
                                Dbgl($"type: {idx} age: {aa.Age} {ab.Age} preg chance: {chance}");
                                if (UnityEngine.Random.Range(0, 100) <= chance)
                                {
                                    Dbgl($"birthing");
                                    __instance.AddAnimal(idx, out tipsId);
                                    typeof(AnimalinFarm).GetField("age", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(aa, aa.Data.StandardPoint);
                                    typeof(AnimalinFarm).GetField("age", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(ab, ab.Data.StandardPoint);
                                    newAnimals[idx]++;
                                }
                                aifs[idx].Remove(aa);
                                aifs[idx].Remove(ab);
                            }
                        } 
                    }
                    foreach(KeyValuePair<int,int> kvp in newAnimals)
                    {
                        if(kvp.Value > 0)
                        {
                            string name = TextMgr.GetStr(AnimalData.GetDataByDataId(kvp.Key).NameId, -1);
                            Dbgl($"sending tips ui");
                            Singleton<TipsMgr>.Instance.SendSystemTip($"{(kvp.Value > 1 ? kvp.Value + " " + name + "s" : "A " + name)} gave birth!");
                        }
                    }
                    return false;
                }
            }
        }
    }
}
