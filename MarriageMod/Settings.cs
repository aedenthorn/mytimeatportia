using Pathea.ActorNs;
using Pathea.FavorSystemNs;
using Pathea.MG;
using System.Collections.Generic;
using UnityEngine;
using UnityModManagerNet;

namespace MarriageMod
{
    public class Settings : UnityModManager.ModSettings
    {

        public int CurrentSpouse { get; set; } = 0;
        public bool SpousesKiss { get; set; } = false;
        public int MinKissingInterval { get; set; } = 5;
        public float MaxKissingDistance { get; set; } = 5f;
        public bool KissSound { get; set; } = true;
        public int RingsPerMonth { get; set; } = 5;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}