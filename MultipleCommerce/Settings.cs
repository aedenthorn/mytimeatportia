﻿using System;
using UnityModManagerNet;

namespace MultipleCommerce
{
    public class Settings : UnityModManager.ModSettings
    {

        public int NumberCommerceOrders { get; set; } = 4;
        public int NumberBigOrders { get; set; } = 2;
        public int NumberSpecialOrders { get; set; } = 1;
        public bool AddFishSpecialOrders { get; set; } = false;
        public bool RandomOrderLevel { get; set; } = false;
        public bool MoreDiverseOrders { get; set; } = false;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}