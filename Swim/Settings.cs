using UnityModManagerNet;

namespace Swim
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool animateSwim = false;
        public bool doSwim = true;
        public float bobMagnitude = 1f;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}