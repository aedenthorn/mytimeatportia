using Harmony12;
using Pathea;
using Pathea.OptionNs;
using Pathea.VoiceNs;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace ChineseVoice
{
    public class Main
    {
        public static Settings settings { get; private set; }

        public class Item
        {
            public int voiceId;

            public int voiceLineId;

            public int audioId;

            public int audioIdCN;
        }

        public static List<Item> list = new List<Item>();

        public static bool enabled;
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
            settings.AlwaysChinese = GUILayout.Toggle(settings.AlwaysChinese, "Always use Chinese voice lines", new GUILayoutOption[0]);
        }
        [HarmonyPatch(typeof(VoiceModule), "GetVoiceAudio")]
        static class VoiceModule_GetVoiceAudio_Patch
        {
            static bool Prefix(ref int __result, int voiceId, int voiceLineId = 0)
            {
                if (!enabled)
                    return true;

                Language voiceLanguage = Singleton<OptionsMgr>.Instance.VoiceLanguage;
                if (LanguageUtils.IsChinese(voiceLanguage))
                    return true;

                if (voiceId == 0 || voiceLineId == 0)
                {
                    __result = 0;
                    return false;
                }
                Item item2 = list.Find(delegate (Item item)
                {
                    if (voiceLineId <= 0)
                    {
                        return item.voiceId == voiceId;
                    }
                    return item.voiceId == voiceId && item.voiceLineId == voiceLineId;
                });
                if (item2 == null)
                {
                    __result = 0;
                    return false;
                }
                if(item2.audioId <= 0 || settings.AlwaysChinese)
                    __result = item2.audioIdCN; 
                else
                    __result = item2.audioId;

                return false;
            }
        }
        [HarmonyPatch(typeof(VoiceModule), "Load")]
        static class VoiceModule_Load_Patch
        {
            static void Postfix()
            {
                Language voiceLanguage = Singleton<OptionsMgr>.Instance.VoiceLanguage;
                
                list = new List<Item>();
                SqliteDataReader sqliteDataReader = LocalDb.cur.ReadFullTable("Voice");
                while (sqliteDataReader.Read())
                {
                    Item item = new Item();
                    item.voiceId = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("TranID"));
                    item.voiceLineId = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("VoiceID"));
                    item.audioId = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("VoiceEN"));
                    item.audioIdCN = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("VoiceCN"));
                    list.Add(item);
                }
            }
        }
    }

}
