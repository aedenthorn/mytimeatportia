using Harmony12;
using Pathea.NpcRepositoryNs;
using System;

namespace PennyComeBack
{
    public static partial class Main
    {

        [HarmonyPatch(typeof(NpcData), "Parse", new Type[]{})]
        static class NpcData_Parse_Patch
        {
            static void Prefix(NpcData __instance, ref string ___interactStr, int ___id, ref int ___addLike, ref int ___factionId, ref int ___min_Favor)
            {
                if (!enabled)
                    return;

                if (___min_Favor == -1)
                    ___min_Favor = 1;

                if (___id == 4000141) // is penny
                {

                    ___min_Favor = -50;

                    if (___addLike == 0)
                    {
                        ___addLike = 1;
                    }
                    ___interactStr += ",Talk,SendGift,Play,CamCapture,Love,InvitePlay,InviteDate,Action,WantChildren";
                    ___factionId = 2; //emily's
                }
            }
        }
    }
}