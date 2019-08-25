using Harmony12;
using Pathea;
using Pathea.HomeNs;
using Pathea.InputSolutionNs;
using Pathea.ItemSystem;
using Pathea.ModuleNs;
using System;
using UnityEngine;

namespace InstantActions
{
    public partial class Main
    {
        // instant cooking add

        [HarmonyPatch(typeof(CookMachineCtr), "Handler", new Type[] { typeof(ActionType) })]
        static class CookMachineCtr_StopInput_Patch
        {
            static bool Prefix(ActionType type, CookMachineCtr __instance, CookMachine ___cookMachine)
            {
                if (!Main.enabled || !settings.InstantCookInput)
                    return true;
                if (type == ActionType.ActionAttack)
                {
                    ItemObject curUseItem = Module<Player>.Self.bag.itemBar.GetCurUseItem();
                    if (curUseItem != null)
                    {
                        Debug.Log(curUseItem.ItemBase.Name);
                        if (curUseItem.ItemBase.HasInteractTag(1))
                        {
                            ___cookMachine.AddMat(curUseItem.ItemDataId);
                        }
                    }
                    return false;
                }
                return true;
            }
        }

    }
}
