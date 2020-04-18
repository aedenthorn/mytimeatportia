using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony12;
using Pathea.ACT;
using Pathea.ActorNs;
using Pathea.EquipmentNs;
using UnityEngine;

namespace InstantActions
{

    public partial class Main
    {
        [HarmonyPatch(typeof(ACTCutree), "OnEnd")]
        static class ACTCutree_Patch
        {
            static void Prefix(ACTDigStone __instance, ref float ____startTime, ref float ____startCutTime)
            {
                if (!enabled)
                    return;

                ____startTime -= 1f;
                ____startCutTime -= 0.3f;

            }
        }
    }
}
