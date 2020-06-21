using UnityModManagerNet;

namespace SkillLimit
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool animateSwim = false;
        public bool doSwim = true;
        public float swimSpeed = 1f;
        public float bobMagnitude = 1f;
        public float bobSpeed = 1f;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}