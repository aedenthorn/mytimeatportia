using UnityModManagerNet;
namespace ChangeClothes
{
    public class Settings : UnityModManager.ModSettings
    {
        public int[] hairs = new int[]
        {
            0,2,4,6,8,10,12,14
        };
        public int[] clothes = new int[]
        {
            0,2,4,6,8,10,12,14
        };
        public int playerClothes;
        public int playerHair;
        public float playerHeadOffset = 0f;
        public float playerHairOffset = 0f;

        public bool EnablePlayerModelling { get; set; } = false;

        //public int PlayerHair { get; set; } = -1;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}