using Harmony12;
using System;
using Pathea.EG;
using Pathea.ActorNs;
using Pathea.Behavior;
using BehaviorDesigner.Runtime.Tasks;

namespace MarriageMod
{
    public static partial class Main
    {
        // remove jealousy stoppping engagement

        [HarmonyPatch(typeof(EGDate), "Cancel",new Type[] { typeof(EGStopType) })]
        static class EGDate_Cancel_Patch
        {
            static bool Prefix(EGStopType stopType)
            {
                if (!Main.enabled)
                    return true;
                Dbgl("EGDate_Cancel_Patch " + stopType.ToString());
                if (stopType == EGStopType.Jealous)
                    return false;
                return true;
            }
        }
    }
}