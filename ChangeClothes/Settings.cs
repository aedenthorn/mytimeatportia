using System.Collections.Generic;
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

        //public int PlayerHair { get; set; } = -1;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}