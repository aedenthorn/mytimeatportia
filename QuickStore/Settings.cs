﻿using UnityModManagerNet;

namespace QuickStore
{
    public class Settings : UnityModManager.ModSettings
    {
        public string StoreKey { get; set; } = "home";
        public bool SkipConfirm { get; set; } = true;
        public int FactoryStorageSize { get; set; } = 300;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}