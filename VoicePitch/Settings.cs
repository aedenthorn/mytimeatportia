using System.Collections.Generic;
using UnityModManagerNet;

namespace VoicePitch
{
    public class Settings : UnityModManager.ModSettings
    {

        public float AaditPitch { get; set; } = 1.0f;
        public float AckPitch { get; set; } = 1.0f;
        public float Albert_Jr_Pitch { get; set; } = 1.0f;
        public float AlbertPitch { get; set; } = 1.0f;
        public float AlicePitch { get; set; } = 1.0f;
        public float Allen_CarterPitch { get; set; } = 1.0f;
        public float Alliance_SoldierPitch {get; set;} = 1.0f;
        public float AntoinePitch { get; set; } = 1.0f;
        public float ArloPitch { get; set; } = 1.0f;
        public float BobPitch { get; set; } = 1.0f;
        public float Builder_WangPitch { get; set; } = 1.0f;
        public float CarolPitch { get; set; } = 1.0f;
        public float CentPitch { get; set; } = 1.0f;
        public float DanaPitch { get; set; } = 1.0f;
        public float DawaPitch { get; set; } = 1.0f;
        public float DjangoPitch { get; set; } = 1.0f;
        public float DollyPitch { get; set; } = 1.0f;
        public float Dr_XuPitch { get; set; } = 1.0f;
        public float EmilyPitch { get; set; } = 1.0f;
        public float ErwaPitch { get; set; } = 1.0f;
        public float EvergladePitch { get; set; } = 1.0f;
        public float First_ChildPitch { get; set; } = 1.0f;
        public float GalePitch { get; set; } = 1.0f;
        public float GingerPitch { get; set; } = 1.0f;
        public float GustPitch { get; set; } = 1.0f;
        public float HanPitch { get; set; } = 1.0f;
        public float HigginsPitch { get; set; } = 1.0f;
        public float HussPitch { get; set; } = 1.0f;
        public float IsaacPitch { get; set; } = 1.0f;
        public float JackPitch { get; set; } = 1.0f;
        public float LeePitch { get; set; } = 1.0f;
        public float LindaPitch { get; set; } = 1.0f;
        public float LiuwaPitch { get; set; } = 1.0f;
        public float LucyPitch { get; set; } = 1.0f;
        public float MaliPitch { get; set; } = 1.0f;
        public float MarcoPitch { get; set; } = 1.0f;
        public float MarsPitch { get; set; } = 1.0f;
        public float MarthaPitch { get; set; } = 1.0f;
        public float McDonaldPitch { get; set; } = 1.0f;
        public float MeiPitch { get; set; } = 1.0f;
        public float MerlinPitch { get; set; } = 1.0f;
        public float MintPitch { get; set; } = 1.0f;
        public float MollyPitch { get; set; } = 1.0f;
        public float MusaPitch { get; set; } = 1.0f;
        public float Mysterious_ManPitch {get; set;} = 1.0f;
        public float NoraPitch { get; set; } = 1.0f;
        public float OaksPitch { get; set; } = 1.0f;
        public float PaPitch { get; set; } = 1.0f;
        public float Papa_BearPitch { get; set; } = 1.0f;
        public float PauliePitch { get; set; } = 1.0f;
        public float PennyPitch { get; set; } = 1.0f;
        public float PetraPitch { get; set; } = 1.0f;
        public float PhyllisPitch { get; set; } = 1.0f;
        public float PinkyPitch { get; set; } = 1.0f;
        public float PollyPitch { get; set; } = 1.0f;
        public float PresleyPitch { get; set; } = 1.0f;
        public float QiwaPitch { get; set; } = 1.0f;
        public float QQPitch { get; set; } = 1.0f;
        public float RemingtonPitch { get; set; } = 1.0f;
        public float RobotPitch { get; set; } = 1.0f;
        public float Rogue_KnightPitch { get; set; } = 1.0f;
        public float RussoPitch { get; set; } = 1.0f;
        public float RyderPitch { get; set; } = 1.0f;
        public float SamPitch { get; set; } = 1.0f;
        public float SanwaPitch { get; set; } = 1.0f;
        public float ScrapsPitch { get; set; } = 1.0f;
        public float Second_ChildPitch {get; set;} = 1.0f;
        public float SiwaPitch { get; set; } = 1.0f;
        public float SoniaPitch { get; set; } = 1.0f;
        public float SophiePitch { get; set; } = 1.0f;
        public float SweetPitch { get; set; } = 1.0f;
        public float TenPitch { get; set; } = 1.0f;
        public float The_All_Source_AIPitch { get; set; } = 1.0f;
        public float TobyPitch { get; set; } = 1.0f;
        public float TodyPitch { get; set; } = 1.0f;
        public float TouristPitch { get; set; } = 1.0f;
        public float TussPitch { get; set; } = 1.0f;
        public float UrsulaPitch { get; set; } = 1.0f;
        public float WarthogPitch { get; set; } = 1.0f;
        public float Workshop_RepPitch { get; set; } = 1.0f;
        public float WuwaPitch { get; set; } = 1.0f;
        public float YeyePitch { get; set; } = 1.0f;
        public float YoyoPitch { get; set; } = 1.0f;
        public float PlayerPitch { get; set; } = 1f;
        public float DMTR_6000Pitch { get; set; } = 1f;
        public float MasonPitch { get; set; } = 1f;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}