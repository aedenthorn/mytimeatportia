using Harmony12;
using Pathea;
using Pathea.ActorNs;
using Pathea.ModuleNs;
using Pathea.ScenarioNs;
using Pathea.UISystemNs;
using PatheaScriptExt;
using System;

namespace InstantActions
{
    public partial class Main
    {
        //skip Dee Dee cutscene

        [HarmonyPatch(typeof(SceneItemTransfer_IMap), "DoCGBusStopTransfer", new Type[] { })]
        static class SceneItemTransfer_IMap_DoCGBusStopTransfer_Patch
        {
            static bool Prefix(SceneItemTransfer_IMap __instance, SceneItem ___sItem)
            {
                if (!enabled || !settings.InstantDeeDee)
                    return true;
                Module<Player>.Self.actor.TryRideOffAndeSetRidableState(RidableState.Follow, false, null);
                Module<TimeManager>.Self.JumpToTime(Module<TimeManager>.Self.DateTime.AddMinutes((double)__instance.GetTransferTime()), TimeManager.JumpingType.System, true);
                Module<Player>.Self.ChangeStamina((float)__instance.GetTransferStamina());
                Module<ScenarioModule>.Self.TransferToScenario(___sItem.SceneName, __instance.TransPoint.position, __instance.TransPoint.eulerAngles);
                return false;
            }
        }
    }
}
