using Harmony12;
using System;
using UnityEngine;

namespace InstantActions
{
    public partial class Main
    {
        [HarmonyPatch(typeof(Animator), "SetBool", new Type[]{typeof(string),typeof(bool)})]
        static class Animator_SetBool_Patch
        {
            static void Prefix(ref Animator __instance, string name, bool value)
            {
                if (!enabled)
                    return;

                // this works on all animations!

                Dbgl("animation "+ name);
                switch (name)
                {
                    case "Cutstone":
                    case "Cutstone_Cut":
                    case "Cutstone_Stand":
                        __instance.speed = value ? settings.PickAxeSpeed : 1f;
                        break;
                    case "Cuttree":
                    case "Cuttree_Cut":
                    case "Cuttree_Stand":
                        __instance.speed = value ? settings.AxeSpeed : 1f;
                        break;
                }

            }
        }
    }
}
