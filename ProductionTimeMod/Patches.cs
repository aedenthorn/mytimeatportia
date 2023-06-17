using Harmony12;
using Hont;
using Pathea;
using Pathea.FarmFactoryNs;
using Pathea.GameFlagNs;
using Pathea.TipsNs;
using Pathea.UISystemNs;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProductionTimeMod
{
    public partial class Main
    {

        [HarmonyPatch(typeof(FarmFactoryMgr), "Tick")]
        static class FarmFactoryMgr_Tick__Patch
        {
            static bool Prefix(List<FarmFactory> ___factorys)
            {
                if(!enabled || !settings.RealTime) 
                    return true;
                ___factorys.Tick(Time.deltaTime * settings.SpeedMult);
                return false;
            }
        }
        [HarmonyPatch(typeof(FarmFactory), "Tick")]
        static class FarmFactory_Tick__Patch
        {
            static void Prefix(ref float delta)
            {
                if(!enabled || settings.RealTime) 
                    return;
                delta *= settings.SpeedMult;
            }
        }
        [HarmonyPatch(typeof(AutomataMgr), "Update")]
        static class AutomataMgr_Update__Patch
        {
            static bool Prefix(AutomataMgr __instance)
            {
                if(!enabled || !settings.RealTime) 
                    return true;
                AccessTools.Method(typeof(AutomataMgr), "UpdateMachineByTime").Invoke(__instance, new object[] { Time.deltaTime * settings.SpeedMult });
                return false;
            }
        }
        [HarmonyPatch(typeof(AutomataMachineData), "Update")]
        static class AutomataMachineData_Update__Patch
        {
            static void Prefix(ref float mul)
            {
                if(!enabled || settings.RealTime) 
                    return;
                mul *= settings.SpeedMult;
            }
        }
    }
}
