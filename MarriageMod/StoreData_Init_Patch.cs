using Harmony12;
using System;
using Pathea.StoreNs;

namespace MarriageMod
{
    public static partial class Main
    {
        // stops breakups on marriage

        [HarmonyPatch(typeof(StoreData), "Init")]
        static class StoreData_Init_Patch
        {
            static void Prefix(ref string[] ___generalProduct)
            {
                if (!enabled)
                    return;
                if (___generalProduct == null)
                {
                    return;
                }
                for (int i = 0; i < ___generalProduct.Length; i++)
                {
                    if (___generalProduct[i] == "1001_1")
                    {
                        ___generalProduct[i] = $"1001_{settings.RingsPerMonth}";
                    }
                }
            }
        }
    }
}