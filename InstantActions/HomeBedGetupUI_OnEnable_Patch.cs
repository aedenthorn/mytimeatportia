using Harmony12;
using Pathea.MessageSystem;
using Pathea.UISystemNs;
using System;

namespace InstantActions
{
    public partial class Main
    {
        //instant wakeup
        /*
                [HarmonyPatch(typeof(WakeUpMask), "Start")]
                static class WakeUpMask_Start_Patch
                {
                    static bool Prefix()
                    {
                        if (!enabled || !settings.InstantWakeup)
                            return true;
                        MessageManager.Instance.Subscribe("WakeUpScreen", new Action<object[]>(Show));
                        return false;
                    }
                }

                static void Show(object[] o)
                {
                    MessageManager.Instance.Dispatch("WakeUpScreenEnd", null, DispatchType.IMME, 2f);
                    if (o != null && o.Length > 0)
                        ((Action)o[0])();
                }

        [HarmonyPatch(typeof(WakeUpMask), "Show", new Type[] { typeof(GameDateTime), typeof(bool), typeof(float) })]
        static class WakeUpMask_Show_Patch
        {
            static bool Prefix(WakeUpMask __instance, ref float delayTime, ref GameObject ___area, BoolTrue ___boolLogic, ref WhiteCat.Tween.Tweener ___tween, ref CanvasGroup ___canvas)
            {
                if (!enabled || !settings.InstantWakeup)
                    return true;
                ___area.SetActive(true);
                ___canvas.alpha = 0f;
                ___tween.enabled = true;
                ___tween.startDelay = delayTime;
                ___tween.normalizedTime = 0f;
                __instance.transform.SetAsLastSibling();
                return false;
            }
        }
            static void Postfix(ref GameObject ___area, BoolTrue ___boolLogic, ref WhiteCat.Tween.Tweener ___tween)
            {
                if (!enabled || !settings.InstantWakeup)
                    return;
                MessageManager.Instance.Dispatch("WakeUpScreenEnd", null, DispatchType.IMME, 2f);
                ___area.SetActive(false);
                InputSolutionModule.Self.RemoverDisable(___boolLogic);
                AudioModule.Self.StopEffect2D(62);
                AudioModule.Self.ResumeBGM(___boolLogic);
                this.HasWakeUpMask = false;
                if (this.endAction != null)
                {
                    this.endAction();
                    this.endAction = null;
                }
                Singleton<SleepTipMgr>.Self.SleepState(false);
                return;
            }
        }
        */
        [HarmonyPatch(typeof(HomeBedGetupUI), "OnEnable", new Type[] { })]
        static class HomeBedGetupUI_OnEnable_Patch
        {
            static void Postfix()
            {
                if (!enabled || !settings.InstantWakeup)
                    return;
                MessageManager.Instance.Dispatch("UIOtherHomeBedGetUp", null, DispatchType.IMME, 2f);
            }
        }

    }
}
