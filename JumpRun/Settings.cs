using UnityModManagerNet;

namespace JumpRun
{
    public class Settings : UnityModManager.ModSettings
    {

        public float MovementSpeed { get; set; } = 1.0f;
        public float JumpHeight { get; set; } = 1.0f;
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}