using Harmony12;
using Hont;
using Pathea;
using Pathea.ActorNs;
using Pathea.ArchiveNs;
using Pathea.HomeNs;
using Pathea.ModuleNs;
using Pathea.NpcRepositoryNs;
using Pathea.RiderNs;
using Pathea.ScenarioNs;
using Pathea.StoreNs;
using Pathea.SummaryNs;
using Pathea.TipsNs;
using Pathea.WeatherNs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace SaveAnyTime
{
    public partial class Main
    {
        private static void DoSaveFile(bool auto = false)
        {
            if (Player.Self == null || Module<Player>.Self == null || Module<Player>.Self.actor == null)
                return;

            string path = GetSavesPath();

            Dbgl($"Checking directory {path}");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (!Directory.Exists(path))
            {
                Dbgl($"Directory {path} does not exist and could not be created!");
                return;
            }

            isSaving = true;


            Dbgl("Building save file");

            SummaryPlayerIdentity curPlayerIdentity = Module<SummaryModule>.Self.GetCurPlayerIdentity();
            DateTime now = DateTime.Now;
            TimeSpan totalPlayTime = Module<SummaryModule>.Self.GetTotalPlayTime();
            GameDateTime dateTime = Module<TimeManager>.Self.DateTime;
            //Dbgl("Building base name");
            string fileName = (string)Singleton<Archive>.Instance.GetType().GetMethod("GenSaveFileName", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Singleton<Archive>.Instance, new object[] { curPlayerIdentity, now, dateTime, totalPlayTime });
            //Dbgl(fileName);
            string name = Module<Player>.Self.ActorName;
            //Dbgl(name);
            string timeOfDay = $"{dateTime.Hour}h{dateTime.Minute}m";
            //Dbgl(timeOfDay);
            string sceneName = Module<ScenarioModule>.Self.CurrentScenarioName;
            //Dbgl(sceneName);
            string position = Module<Player>.Self.GamePos.ToString("F4").Trim(new char[] { '(', ')' });
            fileName = $"{name}_{fileName}_{timeOfDay}_{sceneName}_{position}{(auto?"_auto":"")}";
            //Dbgl(fileName);
            string filePath = Path.Combine(path, fileName);

            // meta file

            Quaternion playerRot = Player.Self.GameRot;

            List<NPCMeta> npcs = new List<NPCMeta>();
            foreach (NpcData data in Module<NpcRepository>.Self.NpcInstanceDatas)
            {
                int instanceId = data.id;
                Actor actor = Module<ActorMgr>.Self.Get(instanceId);
                if (actor == null)
                    continue;
                string scene = actor.SceneName;
                string pos = actor.gamePos.ToString().Trim(new char[] { '(', ')' });
                NPCMeta npc = new NPCMeta();
                npc.id = instanceId;
                npc.scene = scene;
                npc.pos = pos;
                npcs.Add(npc);
            }
            List<RideableMeta> rideables = new List<RideableMeta>();
            foreach (int uid in Module<RidableModuleManager>.Self.GetAllRidableUid())
            {
                IRidable r = Module<RidableModuleManager>.Self.GetRidable(uid);

                if (r == null)
                    continue;
                string pos = r.GetPos().ToString().Trim(new char[] { '(', ')' });
                RideableMeta rideable = new RideableMeta
                {
                    id = uid,
                    pos = pos,
                    state = r.GetRidableState().ToString()
                };
                rideables.Add(rideable);
            }

            List<StoreMeta> stores = new List<StoreMeta>();
            foreach (Store store in AccessTools.FieldRefAccess<StoreManagerV40, List<Store>>(Module<StoreManagerV40>.Self, "storeList"))
            {
                stores.Add(new StoreMeta()
                {
                    id = store.id,
                    money = store.ownMoney,
                    recycleCount = store.recycleCount
                });
            }


            SaveMeta save = new SaveMeta();
            save.playerRot = playerRot.ToString();
            save.NPClist = npcs;
            save.RideableList = rideables;
            save.StoreList = stores;
            save.FishBowlConsumeHour = (int)typeof(FishBowl).GetField("consumeHour", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            save.WeatherState = (int)Module<WeatherModule>.Self.CurWeatherState;
            save.CurPriceIndex = Module<StoreManagerV40>.Self.CurPriceIndex;

            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(SaveMeta));


            Dbgl("Saving file " + filePath);

            Singleton<Archive>.Instance.SaveArchive(filePath);

            var path2 = Path.Combine(GetSavesPath(), $"{fileName}.xml");
            FileStream file = File.Create(path2);
            writer.Serialize(file, save);
            file.Close();

            if (auto)
            {
                Dbgl("Getting old autosaves");
                string createPlayerTime = (string)Singleton<Archive>.Instance.GetType().GetMethod("GenDateFilename", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Singleton<Archive>.Instance, new object[] { curPlayerIdentity.createPlayerTime });

                List<string> autoFiles = new List<string>();
                foreach (CustomSaveFile csf in saveFiles)
                {
                    if (csf.auto && createPlayerTime == csf.creationTime)
                    {
                        autoFiles.Add(Path.Combine(GetSavesPath(), $"{csf.fileName}"));
                    }
                }
                int del = 0;
                while(autoFiles.Count - del >= settings.maxAutoSaves)
                {
                    File.Delete(autoFiles[del]);
                    File.Delete(autoFiles[del] +".xml");
                    del++;
                }
            }

            DoBuildSaveList();
            isSaving = false;
            resetLastSave();
            Singleton<TipsMgr>.Instance.SendImageTip($"Game saved!", MessageUITipImageAssets.ImageType.NewCreationInBook, 0);

        }
    }
}
