using Harmony12;
using Pathea;
using Pathea.HomeNs;
using Pathea.ItemSystem;
using Pathea.ModuleNs;
using Pathea.ScenarioNs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BuildAnywhere
{
    public partial class Main
    {


        [HarmonyPatch(typeof(Region), "GetUnitByCell")]
        static class Region_GetUnitByCell_Patch
        {
            static bool Prefix(Region __instance, CellIndex index, ref Unit __result)
            {
                if (!enabled)
                    return true;
                if (!__instance.IsValidCell(index))
                {
                    //Dbgl("Getting outside item");
                    if (outsideUnits.ContainsKey(index))
                    {
                        Dbgl($"Getting outside unit ({outsideUnits.Count})");
                        __result = outsideUnits[index].unit;
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Region), "GetItemByCellIndex")]
        static class Region_GetItemByCellIndex_Patch
        {
            static bool Prefix(Region __instance, CellIndex index, ref ItemObject __result)
            {
                if (!enabled)
                    return true;
                if (!__instance.IsValidCell(index))
                {
                    //Dbgl($"Getting outside item ({outsideUnits.Count})");

                    if (outsideUnits.ContainsKey(index))
                    {
                        Dbgl("Got outside item");
                        __result = outsideUnits[index].unitObjInfo.Item;
                        return false;
                    }
                }
                return true;
            }
        }


        //[HarmonyPatch(typeof(Region), "GetUnitPutInfo")]
        static class Region_GetUnitPutInfo_Patch
        {
            static bool Prefix(Unit item, ref ItemPutInfo __result)
            {
                if (!enabled)
                    return true;

                Slot slot = outsideUnits.Values.ToList().Find((Slot e) => e.unit == item);
                if (slot != null)
                {
                    Dbgl($"Got outside put info");
                    __result = slot.info;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Region), "PutDownUnit")]
        static class Region_PutDownUnit_Patch
        {
            static void Prefix(Region __instance, Unit unit, ItemPutInfo info, ItemObject item, bool immobile, int fromArchive = -1)
            {
                if (!enabled)
                    return;
                if (!__instance.IsValidCell(info.cellIndex))
                {
                    ItemHomeSystemUnitCmpt component = item.GetComponent<ItemHomeSystemUnitCmpt>();
                    Area area = Module<UnitFactory>.Self.GetArea(info.cellIndex, info.areaRot, component, component.Rotate > 0, fromArchive);

                    Slot slot = new Slot
                    {
                        unit = unit,
                        info = info,
                        immobile = immobile,
                        unitObjInfo = new UnitObjInfo(item, area)
                    };
                    if (outsideUnits.ContainsKey(info.cellIndex))
                    {
                        outsideUnits[info.cellIndex] = slot;
                    }
                    else
                    {
                        outsideUnits.Add(info.cellIndex, slot);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Region), "PutDownDirSafeCheck")]
        static class Region_PutDownDirSafeCheck_Patch
        {
            static bool Prefix(Region __instance, ref bool __result, ItemHomeSystemUnitCmpt common)
            {
                if (!enabled || Module<ScenarioModule>.Self.CurrentScenarioName != "Main" )
                    return true;

                Vector3 pos = Player.Self.GamePos;
                __result = true;
                return false;
            }
        }
        
        [HarmonyPatch(typeof(Region), "SetSlot")]
        static class Region_SetSlot_Patch
        {
            static bool Prefix(Region __instance, Area area)
            {
                if (!enabled)
                    return true;
                for (int i = 0; i < area.Length; i++)
                {
                    if (!__instance.IsValidCell(area[i]))
                    {

                        return false;
                    }
                }
                return true;
            }
        }



        [HarmonyPatch(typeof(Region), "HasUnit", new Type[] { typeof(Area) })]
        static class Region_HasUnit_Patch
        {
            static bool Prefix(Region __instance, Area area, ref bool __result)
            {
                if (!enabled || Module<ScenarioModule>.Self.CurrentScenarioName != "Main")
                    return true;

                for (int i = 0; i < area.Length; i++)
                {
                    if (!__instance.IsValidCell(area[i]))
                    {
                        __result = false;
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Region), "IsUnlocked", new Type[] { typeof(Area) })]
        static class Region_IsUnlocked_Patch
        {
            static bool Prefix(Region __instance, Area area, ref bool __result)
            {
                if (!enabled || Module<ScenarioModule>.Self.CurrentScenarioName != "Main")
                    return true;
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(Region), "IsUnlocked", new Type[] { typeof(CellIndex) })]
        static class Region_IsUnlocked_Patch2
        {
            static bool Prefix(ref bool __result)
            {
                if (!enabled || Module<ScenarioModule>.Self.CurrentScenarioName != "Main")
                    return true;
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(Region), "IsLockTakeUp")]
        static class Region_IsLockTakeUp_Patch
        {
            static bool Prefix(ref bool __result, CellIndex index, ref string reason)
            {
                if (!enabled || Module<ScenarioModule>.Self.CurrentScenarioName != "Main" || !outsideUnits.ContainsKey(index))
                    return true;

                __result = false;
                reason = string.Empty;
                return false;
            }
        }

        [HarmonyPatch(typeof(Region), "TakeUpItem")]
        static class Region_TakeUpItem_Patch
        {
            static bool Prefix(Region __instance, ref ItemObject __result, CellIndex index)
            {
                if (!enabled || Module<ScenarioModule>.Self.CurrentScenarioName != "Main")
                    return true;

                if (outsideUnits.ContainsKey(index))
                {
                    Slot slot = outsideUnits[index];

                    if (slot.unit != null)
                    {
                        if (slot.unit.IsLimitCount)
                        {
                            typeof(Region).GetProperty("LimitUnitCount").SetValue(__instance, (int)typeof(Region).GetProperty("LimitUnitCount").GetValue(__instance, null)-1,null);
                        }
                        slot.unit.TakeUp(delegate (int id, int number)
                        {
                            if (id >= 0)
                            {
                                Module<Player>.Self.bag.AddItem(id, number, true, AddItemMode.Default);
                            }
                            else
                            {
                                Module<Player>.Self.bag.ChangeMoney(number, true, 0);
                            }
                        });
                    }

                    var mySlot = AccessTools.FieldRefAccess<Region, List<object>>(__instance, "slots").Find((object o) => typeof(Region).GetNestedType("Slot", BindingFlags.NonPublic | BindingFlags.Instance).GetField("unit").GetValue(o) == slot.unit);
                    if(mySlot != null)
                    {
                        typeof(Region).GetMethod("RemoveSlot", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { mySlot });
                    }
                    if (slot.unit != null && slot.unit.NeedUpdate)
                    {
                        Module<HomeUnitUpdater>.Self.RemoveUpdateableUnit(slot.unit);
                    }
                    outsideUnits.Remove(index);
                    __result = slot.unitObjInfo.Item;
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Region), "PutbackUnitHandle")]
        static class Region_PutbackUnitHandle_Patch
        {
            static bool Prefix(Region __instance, ref bool __result, UnitHandle unitHandle, ItemPutInfo putInfo, bool changed, bool autoReverseDirProtect = false)
            {
                if (!enabled || Module<ScenarioModule>.Self.CurrentScenarioName != "Main")
                    return true;

                Slot slot = CreateSlotFromRSlot(unitHandle);

                if (slot == null)
                {
                    Dbgl("slot is null");
                    return true;
                }
                if (__instance.IsValidCell(putInfo.cellIndex))
                {
                    Dbgl("valid cell");
                    return true;
                }

                CellIndex oldSlot = slot.info.cellIndex;
                slot.info = putInfo;
                ItemHomeSystemUnitCmpt component = slot.unitObjInfo.Item.GetComponent<ItemHomeSystemUnitCmpt>();
                if (changed)
                {
                    slot.unit.PutDownVersion = 71;
                }
                Area area = Module<UnitFactory>.Self.GetArea(putInfo.cellIndex, putInfo.areaRot, component, component.Rotate > 0, slot.unit.PutDownVersion);
                if (area == null)
                {
                    __result = false;
                    return false;
                }
                slot.area = area;
                slot.unitObjInfo = new UnitObjInfo(slot.unitObjInfo.Item, area);
                outsideUnits.Remove(oldSlot);
                if (outsideUnits.ContainsKey(slot.info.cellIndex))
                {
                    outsideUnits.Add(slot.info.cellIndex,slot);
                }
                else
                {
                    outsideUnits[slot.info.cellIndex] = slot;
                }

                var mySlot = CreateRSlotFromSlot(slot);

                typeof(Region).GetMethod("AddSlot", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { mySlot });
                __result = true;
                return false;
            }
        }

    }
}
