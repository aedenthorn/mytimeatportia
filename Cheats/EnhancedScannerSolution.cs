using System.Collections.Generic;
using Pathea;
using Pathea.ActorThingNs;
using Pathea.BusV2Ns;
using Pathea.CameraSystemNs;
using Pathea.HomeItemThingNs;
using Pathea.HomeViewerNs;
using Pathea.InputSolutionNs;
using Pathea.ItemSystem;
using Pathea.MessageSystem;
using Pathea.ModuleNs;
using Pathea.Museum;
using Pathea.RiderNs;
using UnityEngine;

namespace Cheats
{
    public class EnhancedScannerSolution : GamingSolution
    {
        protected override void Update()
        {
            if (Module<PlayerActionModule>.Self.FixedActionSet.gamingEsc.WasReleased || Module<PlayerActionModule>.Self.WasAcionPressed(ActionType.ActionRevealTreasure))
            {
                base.Esc();
                return;
            }
            base.Update();
        }

    }

}