using Harmony12;
using Hont.ExMethod.Collection;
using Pathea;
using Pathea.ActorNs;
using Pathea.ModuleNs;
using Pathea.NpcRepositoryNs;
using System;
using static Pathea.ChildrenNs.ChildrenModule;

namespace MarriageMod
{
    public static partial class Main
    {
        [HarmonyPatch(typeof(ConversationData), "GetData")]
        static class ConversationData_GetData_Patch
        {
            static void Postfix(ref ConversationData __result, int npcID)
            {
                if (!enabled)
                    return;
                Dbgl("ConversationData_GetData_Patch");

                if(__result == null)
                {
                    Gender g = Module<NpcRepository>.Self.GetNpcGender(npcID);
                    if (g == Gender.Female)
                        __result = ConversationData.GetData(4000003);
                    else
                        __result = ConversationData.GetData(4000004);
                }
            }
        }
    }
}