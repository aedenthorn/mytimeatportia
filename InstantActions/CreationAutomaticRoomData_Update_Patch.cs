using Harmony12;
using Pathea.CreationFactory;
using System;
using System.Collections.Generic;

namespace InstantActions
{
    public partial class Main
    {
        // instant text

        [HarmonyPatch(typeof(CreationAutomaticRoomData), "Update", new Type[] { })]
        static class CreationAutomaticRoomData_Update_Patch
        {
            static bool Prefix(CreationAutomaticRoomData __instance, List<AutoCreationData> ___creationList)
            {
                if (!enabled || !settings.InstantCreation)
                    return true;
                if (___creationList.Count > 0)
                {
                    __instance.FinishCreate();
                    return false;

                }
                return true;
            }
        }

    }
}
