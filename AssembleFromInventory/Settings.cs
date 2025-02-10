using UnityModManagerNet;

namespace AssembleFromInventory
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool PullFromStorage = true;
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}