using Harmony12;
using Pathea.AttrNs;
using Pathea.LevelNs;
using System;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace InfiniteLevels
{
    public static class Main
    {
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

            randSeed = (int)DateTime.UtcNow.Ticks + UnityEngine.Random.Range(0, 9999);
            rand = new System.Random(randSeed);
        }

        private static int randSeed = 0;
        private static System.Random rand = new System.Random();

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


        [HarmonyPatch(typeof(RawConstAttrFactory), "Create", new Type[] { typeof(int),typeof(int) })]
        static class RawConstAttrFactory_Create_Patch
        {
            static bool Prefix(int id, int level, ref AttrData __result)
            {
                if (id != 1 || level < 100)
                    return true;

                AttrData attrData = new AttrData();

                int levelDiff = level - 99;
                float hp = Attribs[0] + levelDiff * 21;
                float attack = Attribs[1] + levelDiff * 2;
                float defence = Attribs[2] + levelDiff * 1;

                attrData.Set(AttrType.HpMax, hp);
                attrData.Set(AttrType.Attack, attack);
                attrData.Set(AttrType.Defence, defence);
                attrData.Set(AttrType.Critical, 0.05f);
                attrData.Set(AttrType.CpMax, 300f);
                attrData.Set(AttrType.VpMax, 100f);
                attrData.Set(AttrType.AntiCritical, 0);
                attrData.Set(AttrType.MeleeCriticalAmount, 1.5f);
                attrData.Set(AttrType.RangeCriticalAmount, 1.5f);

                __result = attrData;

                return false;
            }
        }

        [HarmonyPatch(typeof(LevelCurveModule), "GetExp")]
        static class LevelCurveModule_GetExp_Patch
        {
            static bool Prefix(int id, int level, ref int __result)
            {
                if (!enabled || id != 1 || level < 100)
                    return true;

                Dbgl("level: " + level);
                int exp = levelData[0];
                int delta1 = levelData[1];
                int delta2 = levelData[2];
                for (int i = 0; i < level-99; i++)
                {
                    delta2 += 60; // 40,60,80
                    delta1 += delta2;
                    exp += delta1;
                }
                __result = exp;
                return false;
            }
        }

        [HarmonyPatch(typeof(LevelCurveModule), "GetLevel")]
        static class LevelCurveModule_GetLevel_Patch
        {
            static bool Prefix(int id, int totalExp, ref int exp, ref int nextLevelExp, ref int __result)
            {
                if (!enabled || id != 1 || totalExp < levelData[0])
                    return true;

                Dbgl("total XP: " + totalExp);

                int levelXp = levelData[0];
                int delta1 = levelData[1];
                int delta2 = levelData[2];
                __result = 99; // level - 1

                while (true)
                {
                    int lastXp = levelXp;
                    delta2 += 60; // 40,60,80
                    delta1 += delta2;
                    levelXp += delta1;
                    if(levelXp > totalExp)
                    {
                        nextLevelExp = levelXp - lastXp;
                        exp = totalExp - lastXp;
                        break;
                    }
                    __result++;
                }
                return false;
            }
        }

        private static readonly int[] levelData = { 22998850, 643840, 10260 };
        private static readonly int[] Attribs = { 2168, 211, 106 };

        private static readonly bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "InfiniteLevels " : "") + str);
        }
    }
}