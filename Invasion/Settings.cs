using UnityModManagerNet;

namespace Invasion
{
    public class Settings : UnityModManager.ModSettings
    {
        public int ChanceInvade { get; internal set; } = 30;
        public bool AllowKnight { get; internal set; } = false;
        public bool RelationChange { get; internal set; } = true;
        public bool AllowRats { get; internal set; } = false;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}