using Harmony12;
using Pathea;
using Pathea.ActorAction;
using Pathea.MessageSystem;
using System;
using UnityEngine;

namespace MarriageMod
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

                if (___animator.isInitialized)
                {
                    if(str == "Interact_Kiss" && !val)
                        MessageManager.Instance.Dispatch("InteractKissAnimEnd");
                }
            }
        }
    }
}
