using Harmony12;
using Pathea.UISystemNs;
using UnityEngine;

namespace MarriageMod
{
    public static partial class Main
    {
        // add all spouses to bed selection

        [HarmonyPatch(typeof(HomeBedSettingActorSelector), "SetLocalPos")]
        static class HomeBedSettingActorSelector_SetLocalPos_Patch
        {
            
            static bool Prefix(Transform trans, int index, int count)
            {
                if (!enabled)
                    return true;
                float num = (count - 1) / 2f;
                if (index < num)
                {
                    trans.localPosition = new Vector3((index - num / 2f) * 200f, 0f, 0f);
                }
                else
                {
                    trans.localPosition = new Vector3((index - num - num / 2f) * 200f, 200f, 0f);

                }
                return false;
            }
        }
    }
}