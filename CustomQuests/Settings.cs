﻿using System.Collections.Generic;
using UnityModManagerNet;

namespace CustomQuests
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}