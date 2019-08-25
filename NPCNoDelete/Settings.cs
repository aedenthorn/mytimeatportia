using UnityModManagerNet;

namespace NPCNoDelete
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool Aadit { get; internal set; } = true;
        public bool Ginger { get; internal set; } = true;
        public bool Penny { get; internal set; } = true;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}