using Harmony12;
using Pathea.AimSystemNs;
using Pathea.HomeNs;
using Pathea.HomeViewerNs;
using Pathea.ItemSystem;
using Pathea.ModuleNs;
using Pathea.ScenarioNs;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BuildAnywhere
{
    public partial class Main
    {
        [HarmonyPatch(typeof(HomeItemSelector), "ShowPreviewObj")]
        static class HomeItemSelector_ShowPreviewObj_Patch
        {
            static void Postfix(HomeItemSelector __instance, Area ___area, ref ISelector ___focusSelector, ref GameObject ___previewGameObj)
            {
                if (!enabled || !(___focusSelector is FarmViewer) || ___area == null || ___previewGameObj == null || Module<ScenarioModule>.Self.CurrentScenarioName != "Main")
                    return;

                Vector3 vector2 = GetValidPos(___previewGameObj.transform.position);
                if (vector2 != Vector3.zero)
                {
                    ___previewGameObj.transform.position = vector2;
                }
                else
                {
                    return;
                }
                List<GameObject> ___highlights = AccessTools.FieldRefAccess<RegionViewer, List<GameObject>>(___focusSelector as RegionViewer, "highlights");
                for (int j = 0; j < ___area.Length; j++)
                {
                    Vector3 vector = GetValidPos(___highlights[j].transform.position);
                    if (vector != Vector3.zero)
                    {
                        ___highlights[j].transform.position = vector;
                    }
                }
                AccessTools.FieldRefAccess<RegionViewer, List<GameObject>>(___focusSelector as RegionViewer, "highlights") = ___highlights;
            }
        }

        [HarmonyPatch(typeof(HomeItemSelector), "SetFocusInfo", new Type[] { typeof(Vector3), typeof(Rotation), typeof(bool) })]
        static class HomeItemSelector_SetFocusInfo_Patch
        {
            static bool Prefix(HomeItemSelector __instance, Vector3 pos, Rotation newPlayerRotation, bool includeLockArea, List<SelectorItem> ___selectorItems, ItemHomeSystemUnitCmpt ___systemCmpt, ItemObject ___previewItem, PreviewItemType ___previewItemType)
            {
                if (!enabled || Module<ScenarioModule>.Self.CurrentScenarioName != "Main")
                    return true;
                CellIndex newIndex = CellIndex.Invalid;
                string newFocusWallName = null;
                ISelector newSelector = null;
                float num = float.MaxValue;
                for (int i = 0; i < ___selectorItems.Count; i++)
                {
                    HomeRegionType regionType = ___selectorItems[i].selector.GetRegionType();
                    bool intendPutDown = ___previewItem != null && ___previewItemType != PreviewItemType.Axe;
                    if (!intendPutDown || (regionType == HomeRegionType.InWall && ___systemCmpt.ContainsRegionType(HomeRegionType.InWall)) || (regionType != HomeRegionType.InWall && (___systemCmpt.ContainsRegionType(HomeRegionType.InFloor) || ___systemCmpt.ContainsRegionType(HomeRegionType.OutFloor) || ___systemCmpt.ContainsRegionType(HomeRegionType.Farm))))
                    {
                        float num2;
                        CellIndex cellIndex = ___selectorItems[i].selector.GetPosCell(pos, true, out num2);

                        if (cellIndex == CellIndex.Invalid && intendPutDown)
                        {
                            Vector3 vector = (___selectorItems[i].selector as RegionViewer).transform.InverseTransformPoint(pos);
                            float ceilWidth = AccessTools.FieldRefAccess<RegionViewer, float>((___selectorItems[i].selector as RegionViewer), "ceilWidth");
                            cellIndex = new CellIndex((int)(vector.x / ceilWidth), (int)(-vector.z / ceilWidth));
                        }

                        if (num2 > 2f || num2 < -1f)
                        {
                            cellIndex = CellIndex.Invalid;
                        }
                        num2 = Mathf.Abs(num2);
                        if (cellIndex != CellIndex.Invalid && num2 < num)
                        {
                            num = num2;
                            newIndex = cellIndex;
                            newFocusWallName = ___selectorItems[i].wallName;
                            newSelector = ___selectorItems[i].selector;
                        }
                    }
                }
                typeof(HomeItemSelector).GetMethod("SetFocusInfo", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { newIndex, newSelector, newFocusWallName, newPlayerRotation });
                return false;
            }
        }

        //[HarmonyPatch(typeof(HomeItemSelector), "ShowAreaFocus")]
        static class HomeItemSelector_ShowAreaFocus_Patch
        {
            static void Postfix(ref bool ___areaValid, Area ___area)
            {
                if (!enabled || Module<ScenarioModule>.Self.CurrentScenarioName != "Main" || ___area == null || !settings.allowOverlapInWorkshop)
                    return;
                ___areaValid = true;
            }
        }

        [HarmonyPatch(typeof(HomeItemSelector), "FetchTarget")]
        static class HomeItemSelector_FetchTarget_Patch
        {
            static void Postfix(ISelector ___focusSelector, ref HomeTarget __result)
            {
                if (!enabled || Module<ScenarioModule>.Self.CurrentScenarioName != "Main" || __result == null)
                    return;
                if (___focusSelector.GetType() != typeof(FarmViewer) || !outsideUnits.ContainsKey(__result.itemPutInfo.cellIndex))
                {
                    return;
                }

                Vector3 vector = GetValidPos(__result.pos);
                if (vector != Vector3.zero)
                {
                    __result.pos = vector;
                }
            }
        }

    }
}
