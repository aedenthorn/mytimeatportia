using System;
using UnityModManagerNet;

namespace Interact
{
    public class Settings : UnityModManager.ModSettings
    {
        public float MaxInteractDistance { get; set; } = 2;
        public float HugTime { get; set; } = 5f;
        public string HugKey { get; set; } = "right";
        public string KissKey { get; set; } = "left";
        public string MassageKey { get; set; } = "down";
        public int HugRelationshipMin { get; set; } = 10;
        public int KissRelationshipMin { get; set; } = 12;
        public int MassageRelationshipMin { get; set; } = 14;
        public bool AddRelationshipPoints { get; set; } = true;
        public bool ReplaceTeeterTotterGame { get; set; } = true;
        public bool LimitRelationshipPointGain { get; set; } = true;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}