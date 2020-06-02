using UnityModManagerNet;

namespace ShippingBin
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool delayReceipt = true;
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}