using System;
using UnityModManagerNet;

namespace FasterPiggy
{
    public class Settings : UnityModManager.ModSettings
    {
        private float machinePigSpeedMult = 1.5f;
        private float machinePigLiftMult = 2f;
        private int machinePigConsumePercent = 100;

        public float MachinePigSpeedMult
        {
            get
            {
                return this.machinePigSpeedMult;
            }
            set
            {
                this.machinePigSpeedMult = value;
            }
        }
        public float MachinePigLiftMult
        {
            get
            {
                return this.machinePigLiftMult;
            }
            set
            {
                this.machinePigLiftMult = value;
            }
        }

        public int MachinePigConsumePercent
        {
            get
            {
                return this.machinePigConsumePercent;
            }
            set
            {
                this.machinePigConsumePercent = value;
            }
        }

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}