using UnityModManagerNet;

namespace JumpRun
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool multiJump { get; set; } = false;

        public float MovementSpeed { get; set; } = 1.5f;
        public float JumpHeight { get; set; } = 1.5f;
        //public int JumpJumps { get; set; } = 2;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}