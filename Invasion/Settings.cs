using UnityModManagerNet;

namespace Invasion
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool spawnRandomMonsters = false;
        public int spawnRandomMonstersMin = 1;
        public int spawnRandomMonstersMax = 100;
        public int spawnRandomMonstersGroupMax = 5;

        public int ChanceInvade { get; set; } = 30;
        public int ChanceGang { get; set; } = 0;
        public int MinGang { get; set; } = 1;
        public int MaxGang { get; set; } = 2;
        //public bool AllowKnight { get; set; } = false;
        public bool RelationChange { get; set; } = true;
        public bool AllowRats { get; set; } = false;
        public int RelationPointsPerMonsterTier { get; set; } = 5;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}