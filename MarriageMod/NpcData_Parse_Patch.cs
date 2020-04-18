using Harmony12;
using Pathea.NpcRepositoryNs;
using Pathea.ChildrenNs;
using System.Collections.Generic;
using System.Linq;
using System;

namespace MarriageMod
{
    public static partial class Main
    {
        // make everybody dateable
        [HarmonyPatch(typeof(NpcData), "Parse", new Type[] { })]
        static class NpcData_Parse_Patch
        {
            static void Prefix(ref int ___factionId, ref string ___interactStr, int ___id, NpcData __instance, ref int ___addLike)
            {
                if (!enabled)
                    return;
                List<int> l = new List<int> { 4000002, 4000008, 4000102, 4000012 , 4000114 , 4000040 , 4000059, 4000019, 4000014, 4000055, 4000015 , 4000010, 4000041, 4000013, 4000115, 4000141, 4000097 };


                if (l.IndexOf(___id) > -1)
                {
                    if (___addLike == 0)
                    {
                        ___addLike = 1;
                    }
                    if(___factionId < 1)
                        ___factionId = 12;

                    ___interactStr += ",Talk,SendGift,Play,CamCapture,Love,InvitePlay,InviteDate,Action,WantChildren";
                }
                Dbgl((___interactStr));
            }
            static void Postfix(int ___id, NpcData __instance)
            {

            }
        }
    }
}