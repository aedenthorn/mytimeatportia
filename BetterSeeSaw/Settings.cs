using UnityModManagerNet;

namespace BetterSeeSaw
{
    public class Settings : UnityModManager.ModSettings
    {

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}