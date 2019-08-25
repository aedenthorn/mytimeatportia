using BehaviorDesigner.Runtime;
using Harmony12;
using Pathea.Behavior;
using Pathea.MessageSystem;
using System;

namespace InstantActions
{
    public partial class Main
    {
        [HarmonyPatch(typeof(ITAnimation), "OnStart", new Type[] { })]
        static class ITAnimation_OnStart_Patch
        {
            static void Postfix(ITAnimation __instance, SharedString ___ikName, SharedString ___hugAnimName)
            {
                Dbgl("ikName: " + ___ikName.Value+ " hugAnimName: "+ ___hugAnimName.Value);

                MessageManager.Instance.Subscribe("InteractAnimEnd", new Action<object[]>(delegate (object[] o)
                {
                    try
                    {
                        __instance.OnEnd();
                    }
                    catch
                    {

                    }
                }));
            }
        }
    }
}
