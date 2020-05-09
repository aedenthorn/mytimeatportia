using Harmony12;
using Pathea.ActorAction;
using Pathea.MessageSystem;
using System;
using UnityEngine;

namespace InstantActions
{
    public partial class Main
    {

        [HarmonyPatch(typeof(AnimController), "SetBool", new Type[] { typeof(string), typeof(bool) })]
        static class AnimController_SetBool_Patch
        {
            static void Prefix(string str, bool val, ref Animator ___animator, AnimController __instance)
            {
                if (!enabled)
                    return;

                //Dbgl("animation: " + str); 

                if (___animator.isInitialized)
                {

                    //Finger_Win
                    //GuessLose
                    //Screw
                    //Drilling
                    //Chainsaw_Cut
                    //Brush
                    //Stone
                    //Cloth
                    switch (str)
                    {
                        case "Stone":
                        case "Cloth":
                        case "Scissors":
                            //___animator.speed = val ? settings.RPCSpeed : 1f;
                            //if (!val)
                            //    MessageManager.Instance.Dispatch("InteractAnimEnd", null, DispatchType.MANUAL, 2f);
                            break;
                        case "Embrace_Cat":
                        case "Embrace_Chuan":
                        case "Embrace_Dog":
                            ___animator.speed = val ? settings.PetHugSpeed : 1f;
                            if (!val)
                                MessageManager.Instance.Dispatch("InteractAnimEnd");
                            break;
                        case "Petting":
                        case "Petting_Chuan":
                        case "Petting_Dog":
                            ___animator.speed = val ? settings.PetPetSpeed : 1f;
                            if (!val)
                                MessageManager.Instance.Dispatch("InteractAnimEnd");
                            break;
                        case "Horse_Touch":
                            ___animator.speed = val ? settings.HorseTouchSpeed : 1f;
                            if (!val)
                                MessageManager.Instance.Dispatch("InteractAnimEnd");
                            break;
                        case "Hug":
                            ___animator.speed = val ? settings.HugSpeed : 1f;
                            if (!val)
                                MessageManager.Instance.Dispatch("InteractAnimEnd");
                            break;
                        case "Interact_Kiss":
                            ___animator.speed = val ? settings.KissSpeed : 1f;
                            if (!val)
                                MessageManager.Instance.Dispatch("InteractAnimEnd");
                            break;
                        case "Interact_Massage":
                            ___animator.speed = val ? settings.MassageSpeed : 1f;
                            if (!val)
                                MessageManager.Instance.Dispatch("InteractAnimEnd");
                            break;
                        case "Sow":
                            ___animator.speed = val ? settings.SowGatherSpeed : 1f;
                            if (!val)
                                MessageManager.Instance.Dispatch("InteractAnimEnd");
                            break;
                        case "Throw_1":
                            ___animator.speed = val ? settings.Throw1Speed : 1f;
                            if (!val)
                                MessageManager.Instance.Dispatch("InteractAnimEnd");
                            break;
                        case "Throw_2":
                            ___animator.speed = val ? settings.Throw2Speed : 1f;
                            if (!val)
                                MessageManager.Instance.Dispatch("InteractAnimEnd");
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
