using Harmony12;
using Pathea.ActorNs;
using Pathea.BuffNs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Invasion
{
    public partial class Main
    {
        //[HarmonyPatch(typeof(Actor), "Update")]
        static class rmm2_Patch
        {
            static void Prefix(Actor __instance)
            {
                //if (settings.removeBossBuff && (__instance.ActorName.StartsWith("Boss ") || __instance.ActorName.StartsWith("Gang ")) )
                    __instance.Buff.Clear();

            }
        }
    }
}
