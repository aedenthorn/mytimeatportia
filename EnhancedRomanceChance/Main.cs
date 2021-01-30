using Ccc;
using Harmony12;
using Pathea;
using Pathea.ArchiveNs;
using Pathea.FavorSystemNs;
using Pathea.FeatureNs;
using Pathea.ModuleNs;
using Pathea.SummaryNs;
using Pathea.TipsNs;
using System;
using System.IO;
using UnityEngine;
using UnityModManagerNet;

namespace EnhancedRomanceChance
{
    public class Main
    {

        public static bool enabled;
        private static RomanceMeta romanceMeta = new RomanceMeta();

        public static Settings settings { get; private set; }

        private static readonly bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? $"{typeof(Main).Namespace} " : "") + str);
        } 
        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;


            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            //harmony.PatchAll(Assembly.GetExecutingAssembly());
            harmony.Patch(
               original: AccessTools.Method(typeof(FavorRelationshipUtil), nameof(FavorRelationshipUtil.CheckExpress), new Type[] { typeof(FavorObject), typeof(int).MakeByRefType() }),
               prefix: new HarmonyMethod(typeof(Main), nameof(CheckExpress_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(FavorRelationshipUtil), nameof(FavorRelationshipUtil.CheckPropose), new Type[] { typeof(FavorObject), typeof(int).MakeByRefType() }),
               prefix: new HarmonyMethod(typeof(Main), nameof(CheckPropose_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Archive), nameof(Archive.SaveArchive)),
               postfix: new HarmonyMethod(typeof(Main), nameof(Archive_SaveArchive_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Archive), nameof(Archive.LoadArchive)),
               postfix: new HarmonyMethod(typeof(Main), nameof(Archive_LoadArchive_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(FavorBehavior_GiveGift), "Pathea.FavorSystemNs.IFavorBehavior.Execute"),
               postfix: new HarmonyMethod(typeof(Main), nameof(IFavorBehavior_Execute_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(GameMgr), nameof(GameMgr.NewGameEnterCustomScene)),
               prefix: new HarmonyMethod(typeof(Main), nameof(NewGameEnterCustomScene_Prefix))
            );
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label(string.Format("Straight Confession Chance Increase: <b>+{0}%</b>", settings.ConfessPercentChanceIncrease), new GUILayoutOption[0]);
            settings.ConfessPercentChanceIncrease = (int)GUILayout.HorizontalSlider(settings.ConfessPercentChanceIncrease, 0f, 100f, new GUILayoutOption[0]);
            GUILayout.Space(10);
            GUILayout.Label(string.Format("Confession Chance Increase Per Confession: <b>+{0}%</b>", settings.ConfessPercentChanceIncreasePerConfession), new GUILayoutOption[0]);
            settings.ConfessPercentChanceIncreasePerConfession = (int)GUILayout.HorizontalSlider(settings.ConfessPercentChanceIncreasePerConfession, 0f, 100f, new GUILayoutOption[0]);
            GUILayout.Space(20);
            GUILayout.Label(string.Format("Straight Proposal Chance Increase: <b>+{0}%</b>", settings.ProposePercentChanceIncrease), new GUILayoutOption[0]);
            settings.ProposePercentChanceIncrease = (int)GUILayout.HorizontalSlider(settings.ProposePercentChanceIncrease, 0f, 100f, new GUILayoutOption[0]);
            GUILayout.Space(10);
            GUILayout.Label(string.Format("Proposal Chance Increase Per Proposal: <b>+{0}%</b>", settings.ProposePercentChanceIncreasePerProposal), new GUILayoutOption[0]);
            settings.ProposePercentChanceIncreasePerProposal = (int)GUILayout.HorizontalSlider(settings.ProposePercentChanceIncreasePerProposal, 0f, 100f, new GUILayoutOption[0]);
            GUILayout.Space(20);
        }

        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }

        public static void Archive_SaveArchive_Postfix(string filePath)
        {
            if (!enabled)
                return;

            FileStream file = File.Create(GetRomanceMetaFile(filePath));
            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(RomanceMeta));
            writer.Serialize(file, romanceMeta);
            file.Close();
            Dbgl($"Saved romance meta to {GetRomanceMetaFile(filePath)}");
        }
        
        public static void Archive_LoadArchive_Postfix(string filePath)
        {

            if (!enabled)
                return;

            string metaFilePath = GetRomanceMetaFile(filePath);
            if (File.Exists(metaFilePath))
            {
                System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(RomanceMeta));
                StreamReader stream = new StreamReader(metaFilePath);
                romanceMeta = (RomanceMeta)reader.Deserialize(stream);
                stream.Close();
                Dbgl($"Loaded romance meta from {metaFilePath}");
            }
            else
                romanceMeta = new RomanceMeta();
        }

        private static string GetRomanceMetaFile(string filePath)
        {
            return $"{filePath}_RomanceMeta.xml";
        }

        public static bool CheckExpress_Prefix(FavorObject favorObj, ref int failReason, ref bool __result)
        {
            if (!enabled)
                return true;

            if (!Environment.StackTrace.Contains("ExecuteGiveGift"))
                return true;

            //Dbgl($"Checking confession for {favorObj.ID}");

            if (romanceMeta == null)
            {
                Dbgl("Problem getting romance meta data");
                return true;
            }

            FavorRelationshipData refData = FavorRelationshipData.GetRefData((int)favorObj.Relationship);
            if (refData.canExpress)
            {
                float num = (refData.expressChance[1] - refData.expressChance[0]) * (float)(favorObj.FavorValue - refData.MinValue) / (float)(refData.MaxValue - refData.MinValue) + refData.expressChance[0];
                num += Module<FeatureModule>.Self.ModifyFloat(FeatureType.ExpressSucceedRatio, new object[] { num });

                float baseChance = num;

                Dbgl($"Vanilla confession chance: {baseChance }");


                num += settings.ConfessPercentChanceIncrease * 0.01f;

                RomanceCounts rc = romanceMeta.romances.Find(r => r.id == favorObj.ID);
                if (rc != null)
                    num += settings.ConfessPercentChanceIncreasePerConfession * 0.01f * rc.confessions;

                Dbgl($"Modified confession chance: {num}");

                if (new System.Random().NextDouble() < num)
                {
                    __result = true;
                    return false;
                }
            }
            failReason = 6;
            __result = false;
            return false;
        }

        public static bool CheckPropose_Prefix(FavorObject favorObj, ref int failReason, ref bool __result)
        {
            if (!enabled)
                return true;

            if (!Environment.StackTrace.Contains("ExecuteGiveGift"))
                return true;

            //Dbgl($"Checking proposal for {favorObj.ID}");

            if (romanceMeta == null)
            {
                Dbgl("Problem getting romance meta data");
                return true;
            }

            FavorRelationshipData refData = FavorRelationshipData.GetRefData((int)favorObj.Relationship);
            if (refData.canPropose)
            {
                float num = (refData.proposeChance[1] - refData.proposeChance[0]) * (float)(favorObj.FavorValue - refData.MinValue) / (float)(refData.MaxValue - refData.MinValue) + refData.proposeChance[0];

                Dbgl($"Vanilla confession chance: {num}");

                num += settings.ProposePercentChanceIncrease * 0.01f;

                RomanceCounts rc = romanceMeta.romances.Find(r => r.id == favorObj.ID);
                if (rc != null)
                    num += settings.ProposePercentChanceIncreasePerProposal * 0.01f * rc.proposals;

                Dbgl($"Modified confession chance: {num}");

                if (new System.Random().NextDouble() < num)
                {
                    __result = true;
                    return false;
                }
            }
            failReason = 12;
            __result = false;
            return false;
        }

        private static void AddRomanceMeta(int which, int id)
        {
            for(int i = 0; i < romanceMeta.romances.Count; i++)
            {
                if(romanceMeta.romances[i].id == id)
                {
                    if (which == 0)
                        romanceMeta.romances[i].confessions++;
                    else
                        romanceMeta.romances[i].proposals++;
                    return;
                }
            }
            romanceMeta.romances.Add(new RomanceCounts(id, which));
        }
        private static void IFavorBehavior_Execute_Postfix(FavorBehavior_GiveGift __instance, FavorObject favorObject, object[] args, object __result)
        {
            if (!enabled)
                return;

            if (__result == null)
                return;
            GiveGiftResult ggr = (GiveGiftResult)__result;
            if(((int)args[0] != 7000041 && (int)args[0] != 8000019) || ggr.FeeLevel != FeeLevelEnum.Refuse)
            {
                return;
            }
            Dbgl($"rejected gift {args[0]}: feel { ((__result is GiveGiftResult) ? ((GiveGiftResult)__result).FeeLevel : 0) }");

            for (int i = 0; i < romanceMeta.romances.Count; i++)
            {
                if(romanceMeta.romances[i].id == favorObject.ID)
                {
                    if ((int)args[0] == 7000041)
                        romanceMeta.romances[i].confessions++;
                    else
                        romanceMeta.romances[i].proposals++;
                    return;
                }
            }
            romanceMeta.romances.Add(new RomanceCounts(favorObject.ID, (int)args[0]));
        }

        private static void NewGameEnterCustomScene_Prefix()
        {
            if (!enabled)
                return;

            romanceMeta = new RomanceMeta();
        }
    }
}
