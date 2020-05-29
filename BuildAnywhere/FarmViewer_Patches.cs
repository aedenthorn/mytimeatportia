using Harmony12;
using Pathea;
using Pathea.HomeItemThingNs;
using Pathea.HomeNs;
using Pathea.HomeViewerNs;
using Pathea.ItemSystem;
using Pathea.ModuleNs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BuildAnywhere
{
    public partial class Main
    {


        [HarmonyPatch(typeof(FarmViewer), "OnShow")]
        static class FarmModule_AfterDeserialize_Patch
        {
            static void Postfix()
            {
                if (!enabled)
                    return;

                Module<FarmModule>.Self.ForeachUnit(delegate (Unit unit, bool floor) {

                    Slot slot = outsideUnits.Values.ToList().Find((Slot e) => e.unit == unit);
                    if(slot != null)
                    {
                        GameObject go = Module<FarmModule>.Self.GetUnitGameObjectByUnit(unit);
                        if (go == null)
                        {
                            return;
                        }

                        Vector3 pos = go.transform.position;
                        pos = GetValidPos(pos);
                        if (pos != Vector3.zero)
                        {
                            go.transform.position = pos;
                        }
                    }

                });
            }
        }
       
        [HarmonyPatch(typeof(FarmViewer), "TakeUpHomeItem")]
        static class FarmViewer_TakeUpHomeItem_Patch
        {
            static bool Prefix(Unit unit)
            {
                if (!enabled)
                    return true;

                if (Module<TakeHomeItemModule>.Self.IsTaking)
                {
                    return false;
                }

                Slot slot = outsideUnits.Values.ToList().Find((Slot e) => e.unit == unit);
                if (slot == null)
                    return true;


                Dbgl("taking up outside item");

                ItemPutInfo putInfoByUnit = slot.info;

                ItemObject itemByCellIndex = slot.unitObjInfo.Item;
                string resPath = itemByCellIndex.ItemBase.GetMountData().resPath;

                LayeredRegion layeredRegion = (LayeredRegion)typeof(FarmModule).GetField("layeredRegion",BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Module<FarmModule>.Self);
                Region itemLayer = (Region)typeof(LayeredRegion).GetField("itemLayer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(layeredRegion);

                var mySlot = AccessTools.FieldRefAccess<Region, List<object>>(itemLayer, "slots").Find((object o) => typeof(Region).GetNestedType("Slot", BindingFlags.NonPublic | BindingFlags.Instance).GetField("unit").GetValue(o) == slot.unit);

                UnitHandle unitHandle = (UnitHandle)slot;
                if (unitHandle != null)
                {
                    typeof(Region).GetMethod("RemoveSlot", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(itemLayer, new object[] { mySlot });
                    Dbgl("unit handle is not null");
                    Module<TakeHomeItemModule>.Self.TakeUp(new HomeItemThing(true, string.Empty, putInfoByUnit, unitHandle, resPath, itemByCellIndex));
                }
                return false;
            }
        }

    }
}
