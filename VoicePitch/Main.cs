using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BehaviorDesigner.Runtime.Tasks.Basic.UnityAudioSource;
using Harmony12;
using Pathea;
using Pathea.ActorNs;
using Pathea.FavorSystemNs;
using Pathea.MessageSystem;
using Pathea.NpcRepositoryNs;
using Pathea.TipsNs;
using UnityEngine;
using UnityModManagerNet;

namespace VoicePitch
{
    public class Main
    {
        private static bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "InteractMod " : "") + str);
        }
        public static void TpSend(string str = "")
        {
            if (isDebug)
                Singleton<TipsMgr>.Instance.SendSystemTip(str, SystemTipType.warning);
        }
        public static void WriteToModDir(string str = "")
        {
            if (isDebug)
            {
                string path = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\log.txt";

                using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(path, true))
                {
                    file.WriteLine(str);
                }
            }
        }
        public static bool enabled;
        public static Settings settings { get; private set; }

        // Send a1 response to the mod manager about the launch status, success or not.
        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            return true;
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label(string.Format("Pitch for Aadit: <b>{0:F2}x</b>", settings.AaditPitch), new GUILayoutOption[0]);
            settings.AaditPitch = GUILayout.HorizontalSlider((settings.AaditPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Ack: <b>{0:F2}x</b>", settings.AckPitch), new GUILayoutOption[0]);
            settings.AckPitch = GUILayout.HorizontalSlider((settings.AckPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Albert Jr.: <b>{0:F2}x</b>", settings.Albert_Jr_Pitch), new GUILayoutOption[0]);
            settings.Albert_Jr_Pitch = GUILayout.HorizontalSlider((settings.Albert_Jr_Pitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Albert: <b>{0:F2}x</b>", settings.AlbertPitch), new GUILayoutOption[0]);
            settings.AlbertPitch = GUILayout.HorizontalSlider((settings.AlbertPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Alice: <b>{0:F2}x</b>", settings.AlicePitch), new GUILayoutOption[0]);
            settings.AlicePitch = GUILayout.HorizontalSlider((settings.AlicePitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Allen Carter: <b>{0:F2}x</b>", settings.Allen_CarterPitch), new GUILayoutOption[0]);
            settings.Allen_CarterPitch = GUILayout.HorizontalSlider((settings.Allen_CarterPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Alliance Soldier: <b>{0:F2}x</b>", settings.Alliance_SoldierPitch), new GUILayoutOption[0]);
            settings.Alliance_SoldierPitch = GUILayout.HorizontalSlider((settings.Alliance_SoldierPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Antoine: <b>{0:F2}x</b>", settings.AntoinePitch), new GUILayoutOption[0]);
            settings.AntoinePitch = GUILayout.HorizontalSlider((settings.AntoinePitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Arlo: <b>{0:F2}x</b>", settings.ArloPitch), new GUILayoutOption[0]);
            settings.ArloPitch = GUILayout.HorizontalSlider((settings.ArloPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Bob: <b>{0:F2}x</b>", settings.BobPitch), new GUILayoutOption[0]);
            settings.BobPitch = GUILayout.HorizontalSlider((settings.BobPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Builder_Wang: <b>{0:F2}x</b>", settings.Builder_WangPitch), new GUILayoutOption[0]);
            settings.Builder_WangPitch = GUILayout.HorizontalSlider((settings.Builder_WangPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Carol: <b>{0:F2}x</b>", settings.CarolPitch), new GUILayoutOption[0]);
            settings.CarolPitch = GUILayout.HorizontalSlider((settings.CarolPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Cent: <b>{0:F2}x</b>", settings.CentPitch), new GUILayoutOption[0]);
            settings.CentPitch = GUILayout.HorizontalSlider((settings.CentPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Dana: <b>{0:F2}x</b>", settings.DanaPitch), new GUILayoutOption[0]);
            settings.DanaPitch = GUILayout.HorizontalSlider((settings.DanaPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Dawa: <b>{0:F2}x</b>", settings.DawaPitch), new GUILayoutOption[0]);
            settings.DawaPitch = GUILayout.HorizontalSlider((settings.DawaPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Django: <b>{0:F2}x</b>", settings.DjangoPitch), new GUILayoutOption[0]);
            settings.DjangoPitch = GUILayout.HorizontalSlider((settings.DjangoPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Dolly: <b>{0:F2}x</b>", settings.DollyPitch), new GUILayoutOption[0]);
            settings.DollyPitch = GUILayout.HorizontalSlider((settings.DollyPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Dr. Xu: <b>{0:F2}x</b>", settings.Dr_XuPitch), new GUILayoutOption[0]);
            settings.Dr_XuPitch = GUILayout.HorizontalSlider((settings.Dr_XuPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Emily: <b>{0:F2}x</b>", settings.EmilyPitch), new GUILayoutOption[0]);
            settings.EmilyPitch = GUILayout.HorizontalSlider((settings.EmilyPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Erwa: <b>{0:F2}x</b>", settings.ErwaPitch), new GUILayoutOption[0]);
            settings.ErwaPitch = GUILayout.HorizontalSlider((settings.ErwaPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Everglade: <b>{0:F2}x</b>", settings.EvergladePitch), new GUILayoutOption[0]);
            settings.EvergladePitch = GUILayout.HorizontalSlider((settings.EvergladePitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for First_Child: <b>{0:F2}x</b>", settings.First_ChildPitch), new GUILayoutOption[0]);
            settings.First_ChildPitch = GUILayout.HorizontalSlider((settings.First_ChildPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Gale: <b>{0:F2}x</b>", settings.GalePitch), new GUILayoutOption[0]);
            settings.GalePitch = GUILayout.HorizontalSlider((settings.GalePitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Ginger: <b>{0:F2}x</b>", settings.GingerPitch), new GUILayoutOption[0]);
            settings.GingerPitch = GUILayout.HorizontalSlider((settings.GingerPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Gust: <b>{0:F2}x</b>", settings.GustPitch), new GUILayoutOption[0]);
            settings.GustPitch = GUILayout.HorizontalSlider((settings.GustPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Han: <b>{0:F2}x</b>", settings.HanPitch), new GUILayoutOption[0]);
            settings.HanPitch = GUILayout.HorizontalSlider((settings.HanPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Higgins: <b>{0:F2}x</b>", settings.HigginsPitch), new GUILayoutOption[0]);
            settings.HigginsPitch = GUILayout.HorizontalSlider((settings.HigginsPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Huss: <b>{0:F2}x</b>", settings.HussPitch), new GUILayoutOption[0]);
            settings.HussPitch = GUILayout.HorizontalSlider((settings.HussPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Isaac: <b>{0:F2}x</b>", settings.IsaacPitch), new GUILayoutOption[0]);
            settings.IsaacPitch = GUILayout.HorizontalSlider((settings.IsaacPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Jack: <b>{0:F2}x</b>", settings.JackPitch), new GUILayoutOption[0]);
            settings.JackPitch = GUILayout.HorizontalSlider((settings.JackPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Lee: <b>{0:F2}x</b>", settings.LeePitch), new GUILayoutOption[0]);
            settings.LeePitch = GUILayout.HorizontalSlider((settings.LeePitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Linda: <b>{0:F2}x</b>", settings.LindaPitch), new GUILayoutOption[0]);
            settings.LindaPitch = GUILayout.HorizontalSlider((settings.LindaPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Liuwa: <b>{0:F2}x</b>", settings.LiuwaPitch), new GUILayoutOption[0]);
            settings.LiuwaPitch = GUILayout.HorizontalSlider((settings.LiuwaPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Lucy: <b>{0:F2}x</b>", settings.LucyPitch), new GUILayoutOption[0]);
            settings.LucyPitch = GUILayout.HorizontalSlider((settings.LucyPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Mali: <b>{0:F2}x</b>", settings.MaliPitch), new GUILayoutOption[0]);
            settings.MaliPitch = GUILayout.HorizontalSlider((settings.MaliPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Marco: <b>{0:F2}x</b>", settings.MarcoPitch), new GUILayoutOption[0]);
            settings.MarcoPitch = GUILayout.HorizontalSlider((settings.MarcoPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Mars: <b>{0:F2}x</b>", settings.MarsPitch), new GUILayoutOption[0]);
            settings.MarsPitch = GUILayout.HorizontalSlider((settings.MarsPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Martha: <b>{0:F2}x</b>", settings.MarthaPitch), new GUILayoutOption[0]);
            settings.MarthaPitch = GUILayout.HorizontalSlider((settings.MarthaPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for McDonald: <b>{0:F2}x</b>", settings.McDonaldPitch), new GUILayoutOption[0]);
            settings.McDonaldPitch = GUILayout.HorizontalSlider((settings.McDonaldPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Mei: <b>{0:F2}x</b>", settings.MeiPitch), new GUILayoutOption[0]);
            settings.MeiPitch = GUILayout.HorizontalSlider((settings.MeiPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Merlin: <b>{0:F2}x</b>", settings.MerlinPitch), new GUILayoutOption[0]);
            settings.MerlinPitch = GUILayout.HorizontalSlider((settings.MerlinPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Mint: <b>{0:F2}x</b>", settings.MintPitch), new GUILayoutOption[0]);
            settings.MintPitch = GUILayout.HorizontalSlider((settings.MintPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Molly: <b>{0:F2}x</b>", settings.MollyPitch), new GUILayoutOption[0]);
            settings.MollyPitch = GUILayout.HorizontalSlider((settings.MollyPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Musa: <b>{0:F2}x</b>", settings.MusaPitch), new GUILayoutOption[0]);
            settings.MusaPitch = GUILayout.HorizontalSlider((settings.MusaPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Mysterious_Man: <b>{0:F2}x</b>", settings.Mysterious_ManPitch), new GUILayoutOption[0]);
            settings.Mysterious_ManPitch = GUILayout.HorizontalSlider((settings.Mysterious_ManPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Nora: <b>{0:F2}x</b>", settings.NoraPitch), new GUILayoutOption[0]);
            settings.NoraPitch = GUILayout.HorizontalSlider((settings.NoraPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Oaks: <b>{0:F2}x</b>", settings.OaksPitch), new GUILayoutOption[0]);
            settings.OaksPitch = GUILayout.HorizontalSlider((settings.OaksPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Pa: <b>{0:F2}x</b>", settings.PaPitch), new GUILayoutOption[0]);
            settings.PaPitch = GUILayout.HorizontalSlider((settings.PaPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Papa_Bear: <b>{0:F2}x</b>", settings.Papa_BearPitch), new GUILayoutOption[0]);
            settings.Papa_BearPitch = GUILayout.HorizontalSlider((settings.Papa_BearPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Paulie: <b>{0:F2}x</b>", settings.PauliePitch), new GUILayoutOption[0]);
            settings.PauliePitch = GUILayout.HorizontalSlider((settings.PauliePitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Penny: <b>{0:F2}x</b>", settings.PennyPitch), new GUILayoutOption[0]);
            settings.PennyPitch = GUILayout.HorizontalSlider((settings.PennyPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Petra: <b>{0:F2}x</b>", settings.PetraPitch), new GUILayoutOption[0]);
            settings.PetraPitch = GUILayout.HorizontalSlider((settings.PetraPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Phyllis: <b>{0:F2}x</b>", settings.PhyllisPitch), new GUILayoutOption[0]);
            settings.PhyllisPitch = GUILayout.HorizontalSlider((settings.PhyllisPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Pinky: <b>{0:F2}x</b>", settings.PinkyPitch), new GUILayoutOption[0]);
            settings.PinkyPitch = GUILayout.HorizontalSlider((settings.PinkyPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Polly: <b>{0:F2}x</b>", settings.PollyPitch), new GUILayoutOption[0]);
            settings.PollyPitch = GUILayout.HorizontalSlider((settings.PollyPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Presley: <b>{0:F2}x</b>", settings.PresleyPitch), new GUILayoutOption[0]);
            settings.PresleyPitch = GUILayout.HorizontalSlider((settings.PresleyPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Qiwa: <b>{0:F2}x</b>", settings.QiwaPitch), new GUILayoutOption[0]);
            settings.QiwaPitch = GUILayout.HorizontalSlider((settings.QiwaPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for QQ: <b>{0:F2}x</b>", settings.QQPitch), new GUILayoutOption[0]);
            settings.QQPitch = GUILayout.HorizontalSlider((settings.QQPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Remington: <b>{0:F2}x</b>", settings.RemingtonPitch), new GUILayoutOption[0]);
            settings.RemingtonPitch = GUILayout.HorizontalSlider((settings.RemingtonPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Robot: <b>{0:F2}x</b>", settings.RobotPitch), new GUILayoutOption[0]);
            settings.RobotPitch = GUILayout.HorizontalSlider((settings.RobotPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Rogue Knight: <b>{0:F2}x</b>", settings.Rogue_KnightPitch), new GUILayoutOption[0]);
            settings.Rogue_KnightPitch = GUILayout.HorizontalSlider((settings.Rogue_KnightPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Russo: <b>{0:F2}x</b>", settings.RussoPitch), new GUILayoutOption[0]);
            settings.RussoPitch = GUILayout.HorizontalSlider((settings.RussoPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Ryder: <b>{0:F2}x</b>", settings.RyderPitch), new GUILayoutOption[0]);
            settings.RyderPitch = GUILayout.HorizontalSlider((settings.RyderPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Sam: <b>{0:F2}x</b>", settings.SamPitch), new GUILayoutOption[0]);
            settings.SamPitch = GUILayout.HorizontalSlider((settings.SamPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Sanwa: <b>{0:F2}x</b>", settings.SanwaPitch), new GUILayoutOption[0]);
            settings.SanwaPitch = GUILayout.HorizontalSlider((settings.SanwaPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Scraps: <b>{0:F2}x</b>", settings.ScrapsPitch), new GUILayoutOption[0]);
            settings.ScrapsPitch = GUILayout.HorizontalSlider((settings.ScrapsPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Second_Child: <b>{0:F2}x</b>", settings.Second_ChildPitch), new GUILayoutOption[0]);
            settings.Second_ChildPitch = GUILayout.HorizontalSlider((settings.Second_ChildPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Siwa: <b>{0:F2}x</b>", settings.SiwaPitch), new GUILayoutOption[0]);
            settings.SiwaPitch = GUILayout.HorizontalSlider((settings.SiwaPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Sonia: <b>{0:F2}x</b>", settings.SoniaPitch), new GUILayoutOption[0]);
            settings.SoniaPitch = GUILayout.HorizontalSlider((settings.SoniaPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Sophie: <b>{0:F2}x</b>", settings.SophiePitch), new GUILayoutOption[0]);
            settings.SophiePitch = GUILayout.HorizontalSlider((settings.SophiePitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Sweet: <b>{0:F2}x</b>", settings.SweetPitch), new GUILayoutOption[0]);
            settings.SweetPitch = GUILayout.HorizontalSlider((settings.SweetPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Ten: <b>{0:F2}x</b>", settings.TenPitch), new GUILayoutOption[0]);
            settings.TenPitch = GUILayout.HorizontalSlider((settings.TenPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for The All Source AI: <b>{0:F2}x</b>", settings.The_All_Source_AIPitch), new GUILayoutOption[0]);
            settings.The_All_Source_AIPitch = GUILayout.HorizontalSlider((settings.The_All_Source_AIPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Toby: <b>{0:F2}x</b>", settings.TobyPitch), new GUILayoutOption[0]);
            settings.TobyPitch = GUILayout.HorizontalSlider((settings.TobyPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Tody: <b>{0:F2}x</b>", settings.TodyPitch), new GUILayoutOption[0]);
            settings.TodyPitch = GUILayout.HorizontalSlider((settings.TodyPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Tourist: <b>{0:F2}x</b>", settings.TouristPitch), new GUILayoutOption[0]);
            settings.TouristPitch = GUILayout.HorizontalSlider((settings.TouristPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Tuss: <b>{0:F2}x</b>", settings.TussPitch), new GUILayoutOption[0]);
            settings.TussPitch = GUILayout.HorizontalSlider((settings.TussPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Ursula: <b>{0:F2}x</b>", settings.UrsulaPitch), new GUILayoutOption[0]);
            settings.UrsulaPitch = GUILayout.HorizontalSlider((settings.UrsulaPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Warthog: <b>{0:F2}x</b>", settings.WarthogPitch), new GUILayoutOption[0]);
            settings.WarthogPitch = GUILayout.HorizontalSlider((settings.WarthogPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Workshop Rep: <b>{0:F2}x</b>", settings.Workshop_RepPitch), new GUILayoutOption[0]);
            settings.Workshop_RepPitch = GUILayout.HorizontalSlider((settings.Workshop_RepPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Wuwa: <b>{0:F2}x</b>", settings.WuwaPitch), new GUILayoutOption[0]);
            settings.WuwaPitch = GUILayout.HorizontalSlider((settings.WuwaPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Yeye: <b>{0:F2}x</b>", settings.YeyePitch), new GUILayoutOption[0]);
            settings.YeyePitch = GUILayout.HorizontalSlider((settings.YeyePitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
            GUILayout.Label(string.Format("Pitch for Yoyo: <b>{0:F2}x</b>", settings.YoyoPitch), new GUILayoutOption[0]);
            settings.YoyoPitch = GUILayout.HorizontalSlider((settings.YoyoPitch) * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
        }
        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }

        [HarmonyPatch(typeof(Actor), "SpeakByTextId")]
        static class SpeakByTextId_Patch
        {
            static void Postfix(AudioSource ___audioSource, Actor __instance)
            {
                if (!enabled || ___audioSource == null)
                    return;

                ___audioSource.pitch = GetPitch(__instance);

            }
        }
        [HarmonyPatch(typeof(Actor), "SpeakByVoiceIds")]
        static class SpeakByVoiceIds_Patch
        {
            static void Postfix(AudioSource ___audioSource, Actor __instance)
            {
                if (!enabled || ___audioSource == null)
                    return;

                ___audioSource.pitch = GetPitch(__instance);

            }
        }
        [HarmonyPatch(typeof(Actor), "SpeakByVoiceId")]
        static class SpeakByVoiceId_Patch
        {
            static void Postfix(AudioSource ___audioSource, Actor __instance)
            {
                if (!enabled || ___audioSource == null)
                    return;

                ___audioSource.pitch = GetPitch(__instance);

            }
        }

        public static float GetPitch(Actor actor)
        {
            float pitch = 1.0f;
            if (actor.ActorName == "Aadit")
            {
                pitch = settings.AaditPitch;
            }
            else if (actor.ActorName == "Ack")
            {
                pitch = settings.AckPitch;
            }
            else if (actor.ActorName == "Albert Jr.")
            {
                pitch = settings.Albert_Jr_Pitch;
            }
            else if (actor.ActorName == "Albert")
            {
                pitch = settings.AlbertPitch;
            }
            else if (actor.ActorName == "Alice")
            {
                pitch = settings.AlicePitch;
            }
            else if (actor.ActorName == "Allen Carter")
            {
                pitch = settings.Allen_CarterPitch;
            }
            else if (actor.ActorName == "Alliance Soldier")
            {
                pitch = settings.Alliance_SoldierPitch;
            }
            else if (actor.ActorName == "Antoine")
            {
                pitch = settings.AntoinePitch;
            }
            else if (actor.ActorName == "Arlo")
            {
                pitch = settings.ArloPitch;
            }
            else if (actor.ActorName == "Bob")
            {
                pitch = settings.BobPitch;
            }
            else if (actor.ActorName == "Builder Wang")
            {
                pitch = settings.Builder_WangPitch;
            }
            else if (actor.ActorName == "Carol")
            {
                pitch = settings.CarolPitch;
            }
            else if (actor.ActorName == "Cent")
            {
                pitch = settings.CentPitch;
            }
            else if (actor.ActorName == "Dana")
            {
                pitch = settings.DanaPitch;
            }
            else if (actor.ActorName == "Dawa")
            {
                pitch = settings.DawaPitch;
            }
            else if (actor.ActorName == "Django")
            {
                pitch = settings.DjangoPitch;
            }
            else if (actor.ActorName == "Dolly")
            {
                pitch = settings.DollyPitch;
            }
            else if (actor.ActorName == "Dr. Xu")
            {
                pitch = settings.Dr_XuPitch;
            }
            else if (actor.ActorName == "Emily")
            {
                pitch = settings.EmilyPitch;
            }
            else if (actor.ActorName == "Erwa")
            {
                pitch = settings.ErwaPitch;
            }
            else if (actor.ActorName == "Everglade")
            {
                pitch = settings.EvergladePitch;
            }
            else if (actor.ActorName == "First_Child")
            {
                pitch = settings.First_ChildPitch;
            }
            else if (actor.ActorName == "Gale")
            {
                pitch = settings.GalePitch;
            }
            else if (actor.ActorName == "Ginger")
            {
                pitch = settings.GingerPitch;
            }
            else if (actor.ActorName == "Gust")
            {
                pitch = settings.GustPitch;
            }
            else if (actor.ActorName == "Han")
            {
                pitch = settings.HanPitch;
            }
            else if (actor.ActorName == "Higgins")
            {
                pitch = settings.HigginsPitch;
            }
            else if (actor.ActorName == "Huss")
            {
                pitch = settings.HussPitch;
            }
            else if (actor.ActorName == "Isaac")
            {
                pitch = settings.IsaacPitch;
            }
            else if (actor.ActorName == "Jack")
            {
                pitch = settings.JackPitch;
            }
            else if (actor.ActorName == "Lee")
            {
                pitch = settings.LeePitch;
            }
            else if (actor.ActorName == "Linda")
            {
                pitch = settings.LindaPitch;
            }
            else if (actor.ActorName == "Liuwa")
            {
                pitch = settings.LiuwaPitch;
            }
            else if (actor.ActorName == "Lucy")
            {
                pitch = settings.LucyPitch;
            }
            else if (actor.ActorName == "Mali")
            {
                pitch = settings.MaliPitch;
            }
            else if (actor.ActorName == "Marco")
            {
                pitch = settings.MarcoPitch;
            }
            else if (actor.ActorName == "Mars")
            {
                pitch = settings.MarsPitch;
            }
            else if (actor.ActorName == "Martha")
            {
                pitch = settings.MarthaPitch;
            }
            else if (actor.ActorName == "McDonald")
            {
                pitch = settings.McDonaldPitch;
            }
            else if (actor.ActorName == "Mei")
            {
                pitch = settings.MeiPitch;
            }
            else if (actor.ActorName == "Merlin")
            {
                pitch = settings.MerlinPitch;
            }
            else if (actor.ActorName == "Mint")
            {
                pitch = settings.MintPitch;
            }
            else if (actor.ActorName == "Molly")
            {
                pitch = settings.MollyPitch;
            }
            else if (actor.ActorName == "Musa")
            {
                pitch = settings.MusaPitch;
            }
            else if (actor.ActorName == "Mysterious Man")
            {
                pitch = settings.Mysterious_ManPitch;
            }
            else if (actor.ActorName == "Nora")
            {
                pitch = settings.NoraPitch;
            }
            else if (actor.ActorName == "Oaks")
            {
                pitch = settings.OaksPitch;
            }
            else if (actor.ActorName == "Pa")
            {
                pitch = settings.PaPitch;
            }
            else if (actor.ActorName == "Papa Bear")
            {
                pitch = settings.Papa_BearPitch;
            }
            else if (actor.ActorName == "Paulie")
            {
                pitch = settings.PauliePitch;
            }
            else if (actor.ActorName == "Penny")
            {
                pitch = settings.PennyPitch;
            }
            else if (actor.ActorName == "Petra")
            {
                pitch = settings.PetraPitch;
            }
            else if (actor.ActorName == "Phyllis")
            {
                pitch = settings.PhyllisPitch;
            }
            else if (actor.ActorName == "Pinky")
            {
                pitch = settings.PinkyPitch;
            }
            else if (actor.ActorName == "Polly")
            {
                pitch = settings.PollyPitch;
            }
            else if (actor.ActorName == "Presley")
            {
                pitch = settings.PresleyPitch;
            }
            else if (actor.ActorName == "Qiwa")
            {
                pitch = settings.QiwaPitch;
            }
            else if (actor.ActorName == "QQ")
            {
                pitch = settings.QQPitch;
            }
            else if (actor.ActorName == "Remington")
            {
                pitch = settings.RemingtonPitch;
            }
            else if (actor.ActorName == "Robot")
            {
                pitch = settings.RobotPitch;
            }
            else if (actor.ActorName == "Rogue Knight")
            {
                pitch = settings.Rogue_KnightPitch;
            }
            else if (actor.ActorName == "Russo")
            {
                pitch = settings.RussoPitch;
            }
            else if (actor.ActorName == "Ryder")
            {
                pitch = settings.RyderPitch;
            }
            else if (actor.ActorName == "Sam")
            {
                pitch = settings.SamPitch;
            }
            else if (actor.ActorName == "Sanwa")
            {
                pitch = settings.SanwaPitch;
            }
            else if (actor.ActorName == "Scraps")
            {
                pitch = settings.ScrapsPitch;
            }
            else if (actor.ActorName == "Second Child")
            {
                pitch = settings.Second_ChildPitch;
            }
            else if (actor.ActorName == "Siwa")
            {
                pitch = settings.SiwaPitch;
            }
            else if (actor.ActorName == "Sonia")
            {
                pitch = settings.SoniaPitch;
            }
            else if (actor.ActorName == "Sophie")
            {
                pitch = settings.SophiePitch;
            }
            else if (actor.ActorName == "Sweet")
            {
                pitch = settings.SweetPitch;
            }
            else if (actor.ActorName == "Ten")
            {
                pitch = settings.TenPitch;
            }
            else if (actor.ActorName == "The All Source AI")
            {
                pitch = settings.The_All_Source_AIPitch;
            }
            else if (actor.ActorName == "Toby")
            {
                pitch = settings.TobyPitch;
            }
            else if (actor.ActorName == "Tody")
            {
                pitch = settings.TodyPitch;
            }
            else if (actor.ActorName == "Tourist")
            {
                pitch = settings.TouristPitch;
            }
            else if (actor.ActorName == "Tuss")
            {
                pitch = settings.TussPitch;
            }
            else if (actor.ActorName == "Ursula")
            {
                pitch = settings.UrsulaPitch;
            }
            else if (actor.ActorName == "Warthog")
            {
                pitch = settings.WarthogPitch;
            }
            else if (actor.ActorName == "Workshop Rep")
            {
                pitch = settings.Workshop_RepPitch;
            }
            else if (actor.ActorName == "Wuwa")
            {
                pitch = settings.WuwaPitch;
            }
            else if (actor.ActorName == "Yeye")
            {
                pitch = settings.YeyePitch;
            }
            else if (actor.ActorName == "Yoyo")
            {
                pitch = settings.YoyoPitch;
            }

            return pitch;

        }

    }
}
