using Harmony12;
using Pathea.HomeNs;
using Pathea.HomeViewerNs;
using Pathea.ModuleNs;
using Pathea.ScenarioNs;
using Pathea.UISystemNs;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace BuildAnywhere
{
    public partial class Main
    {
        //public static Settings settings { get; private set; }
        public static bool enabled;

        public static Settings settings;
        private static readonly bool isDebug = true;
        private static Dictionary<CellIndex, Slot> outsideUnits = new Dictionary<CellIndex, Slot>();

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "BuildAnywhere " : "") + str);
        }

        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnToggle = OnToggle;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

        }


        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }
        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            //settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            settings.allowOverlapInWorkshop = GUILayout.Toggle(settings.allowOverlapInWorkshop, "Allow overlap in workshop", new GUILayoutOption[0]);
        }
        private static Vector3 GetValidPos(Vector3 pos)
        {
            Ray ray = default;
            RaycastHit[] hits = new RaycastHit[4];
            pos.y = 0;
            ray.origin = pos + Vector3.up * 1000f;
            ray.direction = Vector3.down;
            int num = Physics.RaycastNonAlloc(ray, hits, 999f, 256);
            if (num == 0)
            {
                return Vector3.zero;
            }
            for (int i = 0; i < num; i++)
            {
                RaycastHit raycastHit = hits[i];
                if (raycastHit.collider)
                {
                    return raycastHit.point;
                }
                Dbgl(raycastHit.collider.tag);
            }
            return Vector3.zero;
        }


        [HarmonyPatch(typeof(ModuleMgr), "Terminate")]
        static class ModuleMgr_Terminate_Patch
        {
            static void Postfix()
            {
                if (!enabled)
                    return;
                outsideUnits.Clear();
            }
        }
 
               
        [HarmonyPatch(typeof(UnitViewer), "GetColorCount")]
        static class UnitViewer_GetColorCount_Patch
        {
            static bool Prefix(UnitViewer __instance, MatColorConfig[] ___configs, ref int __result)
            {
                if (!enabled || Module<ScenarioModule>.Self.CurrentScenarioName != "Main")
                    return true;
                try
                {
                    if (___configs == null)
                    {
                        __result = 0;
                        return false;
                    }
                    __result = Mathf.Min(__instance.unit.CustomColorCount, ___configs.Length);
                    return false;
                }
                catch
                {
                    __result = 0;
                    return false;
                }
            }
        }


        private static object CreateRSlotFromSlot(Slot slot)
        {
            ConstructorInfo ci = typeof(Region).GetNestedType("Slot", BindingFlags.NonPublic | BindingFlags.Instance).GetConstructor(new Type[] { });
            var mySlot = ci.Invoke(new object[] { });
            mySlot.GetType().GetField("area").SetValue(mySlot, slot.area);
            mySlot.GetType().GetField("info").SetValue(mySlot, slot.info);
            mySlot.GetType().GetField("unit").SetValue(mySlot, slot.unit);
            mySlot.GetType().GetField("unitObjInfo").SetValue(mySlot, slot.unitObjInfo);
            mySlot.GetType().GetField("immobile").SetValue(mySlot, slot.immobile);
            return mySlot;
        }
        private static Slot CreateSlotFromRSlot(object mySlot)
        {
            Slot slot = new Slot
            {
                area = (Area) mySlot.GetType().GetField("area").GetValue(mySlot),
                info = (ItemPutInfo) mySlot.GetType().GetField("info").GetValue(mySlot),
                unit = (Unit) mySlot.GetType().GetField("unit").GetValue(mySlot),
                unitObjInfo = (UnitObjInfo) mySlot.GetType().GetField("unitObjInfo").GetValue(mySlot),
                immobile = (bool) mySlot.GetType().GetField("immobile").GetValue(mySlot)

            };
            return slot;
        }


        private class Slot : UnitHandle
        {
            // Token: 0x04002788 RID: 10120
            public Area area;

            // Token: 0x04002789 RID: 10121
            public ItemPutInfo info;

            // Token: 0x0400278A RID: 10122
            public Unit unit;

            // Token: 0x0400278B RID: 10123
            public UnitObjInfo unitObjInfo;

            // Token: 0x0400278C RID: 10124
            public bool immobile;
        }
    }
}
