using System;
using UnityModManagerNet;

namespace TreasureRevealerMod
{
    public class Settings : UnityModManager.ModSettings
    {
        public float RangeMult { get; set; } = 1f;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}