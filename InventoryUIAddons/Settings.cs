using InControl;
using UnityModManagerNet;

namespace InventoryUIAddons
{
    public class Settings : UnityModManager.ModSettings
    {
        public string TakeAllKey { get; set; } = "space";
        public string GrabHalfModKey { get; set; } = "LeftControl";
        public string GrabOneModKey { get; set; } = "LeftAlt";
        public string ForceUseModKey { get; set; } = "LeftShift";
        public bool StoreToToolbarFirst { get; set; } = false;
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}