using System;
using UnityModManagerNet;

namespace InstantActions
{
    public class Settings : UnityModManager.ModSettings
    {

        public bool InstantTreeKick { get; set; } = true;
        public bool InstantCookInput { get; set; } = true;
        public bool InstantFarmPet { get; set; } = true;
        public bool InstantFertilize { get; set; } = true;
        public bool InstantDeeDee { get; set; } = true;
        public bool InstantWakeup { get; set; } = false;
        public bool InstantStartup { get; set; } = false;
        public bool InstantText { get; set; } = false;
        public bool MoveWhileActing { get; set; } = false;
        public bool InstantCreation { get; set; } = true;
        public float HugSpeed { get; set; } = 2f;
        public float KissSpeed { get; set; } = 3f;
        public float MassageSpeed { get; set; } = 3f;
        public float HorseTouchSpeed { get; set; } = 3f;
        public float PetHugSpeed { get; set; } = 3f;
        public float PetPetSpeed { get; set; } = 3f;
        public float RPCSpeed { get; set; } = 3f;
        public float SowGatherSpeed { get; set; } = 2f;
        public float PickAxeSpeed { get; set; } = 2f;
        public float AxeSpeed { get; set; } = 2f;
        public float Throw1Speed { get; set; } = 2f;
        public float Throw2Speed { get; set; } = 2f;
        public bool InstantFullFertilize { get; set; } = true;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}