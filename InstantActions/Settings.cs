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
        public bool InstantDeeDee { get; internal set; } = true;
        public bool InstantWakeup { get; set; } = false;
        public bool InstantStartup { get; set; } = false;
        public bool InstantText { get; internal set; } = false;
        public bool MoveWhileActing { get; internal set; } = false;
        public bool InstantCreation { get; internal set; } = true;
        public float HugSpeed { get; internal set; } = 2f;
        public float KissSpeed { get; internal set; } = 3f;
        public float MassageSpeed { get; internal set; } = 3f;
        public float HorseTouchSpeed { get; internal set; } = 3f;
        public float PetHugSpeed { get; internal set; } = 3f;
        public float PetPetSpeed { get; internal set; } = 3f;
        public float RPCSpeed { get; internal set; } = 3f;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}