using Pathea.ConfigNs;
using UnityModManagerNet;

namespace MarketFluctuation
{
    public class Settings : UnityModManager.ModSettings
    {
        public float MinIndex = OtherConfig.Self.PriceIndexMin;
        public float MaxIndex = OtherConfig.Self.PriceIndexMax;
        public float MinIndexChange = OtherConfig.Self.PriceIndexChangeMin;
        public float MaxIndexChange = OtherConfig.Self.PriceIndexChangeMax;

        public string ShowCurrentIndex { get; set; } = "delete";
        public string ShowMarketIndexText { get; set; } = "Today's Market Index: {0:F2}%";

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}