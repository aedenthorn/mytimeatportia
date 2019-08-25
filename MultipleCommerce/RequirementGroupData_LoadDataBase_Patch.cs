using Harmony12;
using Pathea.Missions;
using System;

namespace MultipleCommerce
{
    public static partial class Main
    {
        [HarmonyPatch(typeof(RequirementGroupData), "LoadDataBase", new Type[] { })]
        static class RequirementGroupData_LoadDataBase_Patch
        {
            static void Postfix()
            {
                RequirementGroupData r = RequirementGroupData.refDataDic[1002];
                int[] fi = new int[FishInts.Length];
                int index = 42000;
                for (int i = 0; i < FishInts.Length; i++)
                {
                    fi[i] = index++;
                }

                var z = new int[r.requirementIdAry.Length + fi.Length];
                r.requirementIdAry.CopyTo(z, 0);
                fi.CopyTo(z, r.requirementIdAry.Length);
                RequirementGroupData.refDataDic[1002].requirementIdAry = z;
            }
        }
    }
}