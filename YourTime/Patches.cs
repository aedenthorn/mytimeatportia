using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony12;
using Hont;
using Pathea;
using Pathea.ActorNs;
using Pathea.GameFlagNs;
using Pathea.TipsNs;
using Pathea.UISystemNs;
using UnityEngine;

namespace YourTime
{
    public partial class Main
    {
        [HarmonyPatch(typeof(TimeManager), "TimeScale", MethodType.Getter)]
        internal static class TimeManager_TimeScale_Patch
        {
            // Token: 0x06000008 RID: 8 RVA: 0x0000223C File Offset: 0x0000043C
            private static void Postfix(ref float __result)
            {
                if (!enabled)
                {
                    return;
                }
                __result *=  Mathf.Clamp(settings.TimeScaleModifier, 0.1f, 10f);
            }
        }

        [HarmonyPatch(typeof(Player), "Update")]
        static class Player_Patch
        {
            static void Postfix()
            {
                if (!Singleton<GameFlag>.Self.Gaming ||
                    (!Input.GetKeyDown(settings.StopTimeKey) &&
                     !Input.GetKeyDown(settings.SubtractTimeKey) &&
                     !Input.GetKeyDown(settings.AdvanceTimeKey) &&
                     !Input.GetKeyDown(settings.SlowTimeKey) &&
                     !Input.GetKeyDown(settings.SpeedTimeKey)) ||
                    UIStateMgr.Instance.currentState.type != UIStateMgr.StateType.Play ||
                    Player.Self.actor == null
                )
                {
                    return;
                }

                if (Input.GetKeyDown(settings.StopTimeKey))
                {
                    AccessTools.FieldRefAccess<TimeManager,BoolLogic>(TimeManager.Self,"timeStopLogic").Clear();
                    if (TimeManager.Self.TimeStoped)
                    {
                        TimeManager.Self.RemoveTimeStop(LockTime);
                        TimeManager.Self.EnableForStory = true;
                        Singleton<TipsMgr>.Instance.SendImageTip($"Time Started!", MessageUITipImageAssets.ImageType.CalendarRemind, 0);
                    }
                    else
                    {
                        TimeManager.Self.AddTimeStop(LockTime);
                        TimeManager.Self.EnableForStory = false;
                        Singleton<TipsMgr>.Instance.SendImageTip($"Time Stopped!", MessageUITipImageAssets.ImageType.CalendarRemind, 0);
                    }
                }
                else if (Input.GetKeyDown(settings.SubtractTimeKey))
                {
                    GameDateTime dt = TimeManager.Self.DateTime.AddSeconds(-60 * 60);
                    GameTimeSpan t = dt - TimeManager.Self.DateTime;
                    TimeManager.Self.SetDateTime(dt, true, TimeManager.JumpingType.Max);
                }
                else if (Input.GetKeyDown(settings.AdvanceTimeKey))
                {
                    TimeManager.Self.JumpTimeByGameTime(60 * 60);
                }
                else if (Input.GetKeyDown(settings.SlowTimeKey))
                {
                    if (settings.TimeScaleModifier > 0.1f)
                    {
                        if (settings.TimeScaleModifier > 1.5f)
                        {
                            settings.TimeScaleModifier = (float)Math.Round(settings.TimeScaleModifier - 1f);
                        }
                        else
                        {
                            settings.TimeScaleModifier = (float)Math.Round(settings.TimeScaleModifier - 0.1f,1);
                        }
                        Singleton<TipsMgr>.Instance.SendImageTip($"Time speed set to {TimeSpeedString()} speed", MessageUITipImageAssets.ImageType.CalendarRemind, 0);
                        settings.Save(myModEntry);
                    }
                    else
                    {
                        Singleton<TipsMgr>.Instance.SendSystemTip("Time already at slowest speed!", SystemTipType.warning);
                    }
                }
                else if (Input.GetKeyDown(settings.SpeedTimeKey))
                {
                    if (settings.TimeScaleModifier < 10.0f)
                    {
                        if (settings.TimeScaleModifier >= 1f)
                        {
                            settings.TimeScaleModifier = (float)Math.Round(settings.TimeScaleModifier + 1f);
                        }
                        else
                        {
                            settings.TimeScaleModifier = (float)Math.Round(settings.TimeScaleModifier + 0.1f, 1);
                        }
                        Singleton<TipsMgr>.Instance.SendImageTip($"Time speed set to {TimeSpeedString()} speed", MessageUITipImageAssets.ImageType.CalendarRemind, 0);
                        settings.Save(myModEntry);
                    }
                    else
                    {
                        Singleton<TipsMgr>.Instance.SendSystemTip("Time already at fastest speed!", SystemTipType.warning);
                    }
                }
            }
        }
        private static readonly BoolTrue LockTime = new BoolTrue();

    }
}
