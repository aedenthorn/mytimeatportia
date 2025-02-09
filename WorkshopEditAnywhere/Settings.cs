using UnityModManagerNet;

namespace WorkshopEditAnywhere
{
    public class Settings : UnityModManager.ModSettings
    {
        public string MenuKey { get; set; } = "[5]";

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}