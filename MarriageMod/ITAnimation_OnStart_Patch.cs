using BehaviorDesigner.Runtime;
using Harmony12;
using Pathea.Behavior;
using Pathea.MG;
using Pathea.ModuleNs;
using System;
using UnityEngine;

namespace MarriageMod
{
    public static partial class Main
    {
        // set settings spouse to massage spouse

        [HarmonyPatch(typeof(ITAnimation), "OnStart", new Type[] { })]
        static class ITAnimation_OnStart_Patch
        {
            static void Postfix(SharedString ___hugAnimName, SharedActor ___actor)
            {
                if (!enabled)
                    return;


                if (___hugAnimName.Value == "Interact_Massage")
                {
                    settings.CurrentSpouse = ___actor.Value.InstanceId;
                }
            }
        }
    }

}
