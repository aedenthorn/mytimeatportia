using System.Collections.Generic;
using UnityModManagerNet;

namespace CustomMerch
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool isDebug { get; set; } = false;
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}