using UnityModManagerNet;

namespace QuickCollect
{
    public class Settings : UnityModManager.ModSettings
    {
        public string CollectKey { get; set; } = "[0]";

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}