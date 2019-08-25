using Harmony12;
using Pathea.UISystemNs;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine.UI;

namespace InstantActions
{
    public partial class Main
    {
        
        // instant text

        [HarmonyPatch(typeof(TypewriterEffect), "OnStart", new Type[] { })]
        static class TypewriterEffect_OnStart_Patch
        {
            static void Postfix(TypewriterEffect __instance)
            {
                if (!enabled || !settings.InstantText)
                    return;

                MethodInfo dynMethod = __instance.GetType().GetMethod("OnFinish", BindingFlags.NonPublic | BindingFlags.Instance);
                dynMethod.Invoke(__instance, new object[] { });
                return;
            }
        }

    }
}
