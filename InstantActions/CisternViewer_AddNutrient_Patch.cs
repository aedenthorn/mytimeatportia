using Harmony12;
using Pathea;
using Pathea.HomeNs;
using Pathea.HomeViewerNs;
using Pathea.ItemSystem;
using Pathea.ModuleNs;
using Pathea.UISystemNs;
using System;
using UnityEngine;

namespace InstantActions
{
    public partial class Main
    {
        // add nutrient to cistern (irrigation tower)

        [HarmonyPatch(typeof(CisternViewer), "AddNeutrient", new Type[] { })]
        static class CisternViewer_AddNutrient_Patch
        {
            static bool Prefix(PlantingBoxUnit ___cisternUnit)
            {
                if (!enabled || !settings.InstantFertilize)
                    return true;
                ItemObject curUseItem = Module<Player>.Self.bag.itemBar.GetCurUseItem();
                if (curUseItem != null)
                {
                    ItemNutrientCmpt nutri = curUseItem.GetComponent<ItemNutrientCmpt>();
                    if (nutri != null)
                    {
                        UIStateMgr.Instance.ChangeStateByType(UIStateMgr.StateType.Empty, false, null);
                        UIUtils.ShowNumberSelectMinMax(curUseItem.ItemDataId, 1, Mathf.Min(curUseItem.Number, (int)(((float)___cisternUnit.MaxNutrient - ___cisternUnit.CurrentNutrient) / (float)nutri.Point)), 1, string.Empty, delegate (int o)
                        {
                            Module<Player>.Self.bag.itemBar.RemoveCurItem(o);
                            ___cisternUnit.TryAddNutrient((float)(nutri.Point * o), true);
                            UIStateMgr.Instance.PopState(false);
                        }, delegate ()
                        {
                            UIStateMgr.Instance.PopState(false);
                        }, false, 0, string.Empty);
                    }
                }
                return false;
            }
        }

    }
}
