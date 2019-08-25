using Harmony12;
using Pathea;
using Pathea.HomeNs;
using Pathea.HomeViewerNs;
using Pathea.ItemSystem;
using Pathea.ModuleNs;
using System;
using UnityEngine;

namespace InstantActions
{
    public partial class Main
    {
        // add nutrient to planting box

        [HarmonyPatch(typeof(PlantingBoxViewer), "AddNeutrient", new Type[] { })]
        static class PlantingBoxViewer_AddNeutrient_Patch
        {
            static bool Prefix(PlantingBoxUnit ___boxUnit, UnityEngine.Transform ___addNutriParticleParent)
            {
                if (!enabled || !settings.InstantFertilize)
                    return true;
                ItemObject curUseItem = Module<Player>.Self.bag.itemBar.GetCurUseItem();
                if (curUseItem != null)
                {
                    ItemNutrientCmpt nutri = curUseItem.GetComponent<ItemNutrientCmpt>();
                    if (nutri != null && ___boxUnit.CanAddNutrient(nutri.Point))
                    {
                        Module<Player>.Self.bag.itemBar.RemoveCurItem(1);
                        ___boxUnit.TryAddNutrient((float)nutri.Point, true);
                        GameObject addNutriParticle = GameUtils.AddChildToTransform(___addNutriParticleParent, "FX_ApplyFertilizer", false, AssetType.Effect);
                        ParticleSystem componentInChildren = addNutriParticle.GetComponentInChildren<ParticleSystem>();
                        componentInChildren.Play();
                        Singleton<TaskRunner>.Instance.RunDelayTask(componentInChildren.main.duration, false, delegate
                        {
                            UnityEngine.Object.Destroy(addNutriParticle);
                        });
                    }
                }

                return false;
            }
        }

    }
}
