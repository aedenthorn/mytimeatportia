using Harmony12;
using Pathea;
using Pathea.InputSolutionNs;
using Pathea.MessageSystem;
using Pathea.ModuleNs;
using Pathea.PlayerOperateNs;
using Pathea.RiderNs;
using System;

namespace InstantActions
{
    public partial class Main
    {
        //instant tree kicking

        [HarmonyPatch(typeof(Pathea.TreeInteraction), "Handler", new Type[] {typeof(ActionType) })]
        static class TreeInteraction_Handler_Patch
        {
            static bool Prefix()
            {
                if (!Main.enabled || !settings.InstantTreeKick)
                    return true;
                if (!PlayerOperateModule.Self.CheckStaminaAndSendTooltip(OperateType.KickTree, 1, false))
                {
                    return true;
                }
                if (Module<Player>.Self.actor.RiderState == RiderState.RideOn)
                {
                    return true;
                }
                PlayerOperateModule.Self.GainExp(OperateType.KickTree);
                PlayerOperateModule.Self.ConsumeStamina(OperateType.KickTree, 1, 1f);
                MessageManager.Instance.Dispatch("KickTree", null, DispatchType.IMME, 0f);
                return false;
            }
        }

    }
}
