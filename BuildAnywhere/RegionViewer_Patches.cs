using Harmony12;
using Pathea.HomeNs;
using Pathea.HomeViewerNs;
using Pathea.ModuleNs;
using Pathea.ScenarioNs;
using System;
using System.Reflection;
using UnityEngine;

namespace BuildAnywhere
{
    public partial class Main
    {


        //[HarmonyPatch(typeof(RegionViewer), "CreateUnitGameObj")]
        static class FarmViewer_CreateUnitGameObj_Patch
        {
            static void Postfix(RegionViewer __instance, UnitObjInfo objInfo)
            {
                if (!enabled || Module<ScenarioModule>.Self.CurrentScenarioName != "Main" || !(__instance is FarmViewer))
                    return;
                Vector3 pos = objInfo.go.transform.position;
                pos = GetValidPos(pos);
                if (pos != Vector3.zero)
                {
                    objInfo.go.transform.position = pos;
                }
            }
        }


        [HarmonyPatch(typeof(RegionViewer), "GetIndexFromWroldPos")]
        static class RegionViewer_GetIndexFromWroldPos_Patch
        {
            static void Postfix(RegionViewer __instance, ref Vector3 worldPos, ref float dis, ref CellIndex __result, float ___ceilWidth)
            {
                if (!enabled || Module<ScenarioModule>.Self.CurrentScenarioName != "Main" || !(__instance is FarmViewer))
                    return;
                Vector3 vector = GetValidPos(worldPos);
                if (vector != Vector3.zero)
                {
                    dis = (worldPos - vector).y;
                }
                if (__result == CellIndex.Invalid)
                {
                    Vector3 vector2 = __instance.transform.InverseTransformPoint(worldPos);
                    __result = new CellIndex((int)(vector2.x / ___ceilWidth), (int)(-vector2.z / ___ceilWidth));
                }
            }
        }
        [HarmonyPatch(typeof(RegionViewer), "FreshLocalPosition")]
        static class RegionViewer_FreshLocalPosition_Patch
        {
            static void Postfix(RegionViewer __instance, ItemPutInfo info, ref Transform transform)
            {
                if (!enabled || Module<ScenarioModule>.Self.CurrentScenarioName != "Main" || !(__instance is FarmViewer) || !outsideUnits.ContainsKey(info.cellIndex))
                    return;

                Vector3 vector = GetValidPos(transform.position);
                if (vector != Vector3.zero)
                {
                    transform.position = vector;
                }
            }
        }


        [HarmonyPatch(typeof(RegionViewer), "CreateGameObj", new Type[] { typeof(string), typeof(Area), typeof(ItemPutInfo), typeof(bool) })]
        static class RegionViewer_CreateGameObj_Patch
        {
            static bool Prefix(RegionViewer __instance, string path, Area area, ItemPutInfo info, ref GameObject __result, bool isFloorLayer)
            {

                Dbgl($"path {path}, ");
                return true;
                if (!enabled || Module<ScenarioModule>.Self.CurrentScenarioName != "Main" || !outsideUnits.ContainsKey(info.cellIndex))
                    return true;


                GameObject gameObject = (GameObject)typeof(RegionViewer).GetMethod("CreateGameObj", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(string) },new ParameterModifier[0]).Invoke(__instance, new object[] { path });
                typeof(RegionViewer).GetMethod("FreshLocalPosition", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { gameObject.transform, info, area });
                typeof(RegionViewer).GetMethod("FreshLocalRotation", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { gameObject.transform, info });

                UnitViewer unitViewer = gameObject.GetComponentInChildren<UnitViewer>();
                if (unitViewer == null)
                {
                    unitViewer = gameObject.AddComponent<UnitViewer>();
                }
                if (unitViewer != null)
                {
                    Unit unitByCell = outsideUnits[info.cellIndex].unit;

                    unitViewer.SetUnit(unitByCell);
                    typeof(RegionViewer).GetMethod("SetRegionName", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { unitViewer });

                    if (!unitByCell.immobile && !(unitByCell is FloorCell))
                    {
                        MethodInfo method = __instance.GetType().GetMethod("TakeUpHomeItem", BindingFlags.NonPublic | BindingFlags.Instance);
                        Action<Unit> action = (Action<Unit>)Delegate.CreateDelegate(typeof(Action<Unit>), __instance, method);
                        unitViewer.InitHomeItemThing(action);
                    }
                    if (!unitByCell.immobile && unitByCell.CustomColorCount > 0)
                    {
                        unitViewer.InitColorChange();
                    }
                }
                gameObject.SetActive(true);
                __result = gameObject;
                return false;
            }
        }


        [HarmonyPatch(typeof(RegionViewer), "Pathea.HomeViewerNs.ISelector.GetPosCell")]
        static class ISelector_GetPosCell_Patch
        {
            static void Prefix(ISelector __instance, ref bool includeLockArea)
            {
                if (!enabled || Module<ScenarioModule>.Self.CurrentScenarioName != "Main" || !(__instance is FarmViewer))
                    return;
                includeLockArea = true;
            }
            static void Postfix(ISelector __instance, ref CellIndex __result, Vector3 pos, float ___ceilWidth)
            {
                if (!enabled || !(__instance is FarmViewer))
                    return;
                if(__result == CellIndex.Invalid)
                {
                    Vector3 vector2 = (__instance as Component).transform.InverseTransformPoint(pos);
                    __result = new CellIndex((int)(vector2.x / ___ceilWidth), (int)(-vector2.z / ___ceilWidth));
                }
            }
        }
    }
}
