using BehaviorDesigner.Runtime;
using Harmony12;
using Pathea;
using Pathea.ActorNs;
using Pathea.Behavior;
using Pathea.Conversations;
using Pathea.Interactive;
using Pathea.MessageSystem;
using Pathea.ModuleNs;
using Pathea.NpcRepositoryNs;
using Pathea.UISystemNs;
using PatheaScriptExt;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace PennyComeBack
{
    public static partial class Main 
    {
        public static List<ITDecorator> emilyITActions;
        public static SharedVariable emilyInteractive;
        public static SharedVariable emilyInteractiveJealous;
        public static SharedVariable emilyEGMood;
        public static SharedVariable emilyEGForce;
        public static SharedVariable emilyEGActor;
        public static SharedVariable emilyEGTarget;
        public static SharedVariable emilyEGDate;

        [HarmonyPatch(typeof(ActorMgr), "OnLoad")]
        static class ActorMgr_OnLoad_Patch
        {
            static void Postfix(List<ActorInfo> ___actorInfos)
            {
                foreach(ActorInfo ai in ___actorInfos)
                {

                    //Dbgl("actorinfo "+ai.miniHeadIcon_db + " " + ai.AiName + " " + string.Join(",", (ai.behaviorData == null?new string[] { "null" }:ai.behaviorData)));
                }
            }
        }

        [HarmonyPatch(typeof(Actor), "InitInteractive")]
        static class Actor_InitInteractive_Patch
        {
            static void Postfix(Actor __instance, ref List<ITDecorator> ___mITDecorators, ref int ___mFavoMin, int ___instanceId, ref BehaviorTree ___mBehavior)
            {
                if (___instanceId == EmilyID)
                {
                    emilyITActions = ___mITDecorators;
                    emilyInteractive = ___mBehavior.GetVariable("Interactive");
                    emilyInteractiveJealous = ___mBehavior.GetVariable("InteractiveJealous");
                    emilyEGForce = ___mBehavior.GetVariable("EGForce");
                    emilyEGMood = ___mBehavior.GetVariable("EGMood");
                    emilyEGActor = ___mBehavior.GetVariable("EGActor");
                    emilyEGTarget = ___mBehavior.GetVariable("EGTarget");
                    emilyEGDate = ___mBehavior.GetVariable("EGDate");
                }
                else if (___instanceId == PennyID)
                {
                    SharedActor pennyShared = new SharedActor();
                    pennyShared.SetValue(__instance);

                    ___mFavoMin = 10;
                    ___mITDecorators = emilyITActions;
                    if (___mITDecorators != null && ___mITDecorators.Count > 0)
                    {
                        for (int j = 0; j < ___mITDecorators.Count; j++)
                        {
                            ___mITDecorators[j].actor = pennyShared;
                        }
                    }
                    ___mBehavior.SetVariable("Interactive", pennyShared);
                    ___mBehavior.SetVariable("InteractiveJealous", new SharedBool());
                    ___mBehavior.SetVariableValue("InteractiveJealous", false);
                    ___mBehavior.SetVariable("EGMood", new SharedInt());
                    ___mBehavior.SetVariableValue("EGMood", 0);
                    ___mBehavior.SetVariable("EGForce", new SharedInt());
                    ___mBehavior.SetVariableValue("EGForce", 0);
                    ___mBehavior.SetVariable("EGActor", emilyEGActor);
                    ___mBehavior.SetVariableValue("EGActor", null);
                    ___mBehavior.SetVariable("EGTarget", emilyEGTarget);
                    ___mBehavior.SetVariableValue("EGTarget", null);
                    ___mBehavior.SetVariable("EGDate", emilyEGDate);
                    ___mBehavior.SetVariableValue("EGDate", null);
                }
            }
        }

        [HarmonyPatch(typeof(Actor), "CanInteractive")]
        static class Actor_CanInteractive_Patch
        {
            static void Prefix(ref List<ITDecorator> ___mITDecorators, ref int ___mFavoMin, int ___instanceId, ref BehaviorTree ___mBehavior)
            {
            }
            static void Postfix(bool __result)
            {
                Dbgl("actor can interact? " + __result.ToString());
            }
        }
    }
}