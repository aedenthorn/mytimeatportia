using Harmony12;
using Pathea.Conversations;
using Pathea.ActorNs;
using Pathea.FavorSystemNs;
using System.Collections.Generic;
using System;

namespace MarriageMod
{
    public static partial class Main
    {
        [HarmonyPatch(typeof(ConversationManager), "Talk2Npc", new Type[] { typeof(Actor) })]
        static class ConversationManager_Talk2Npc_Patch
        {
            static void Prefix(Actor actor)
            {
                Dbgl("ActorID: " + actor.InstanceId);
                if(actor != null && IsSpouse(actor.InstanceId) && GetCurrentSpouse() != 0)
                {
                    SetCurrentSpouse(actor.InstanceId);
                }
            }
        }
    }
}