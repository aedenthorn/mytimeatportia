using Harmony12;
using Pathea;
using Pathea.CaiQuanNs;
using System;
using System.Reflection;
using static Pathea.TaskRunner;

namespace InstantActions
{
    public partial class Main
    {
        [HarmonyPatch(typeof(CaiQuanGame), "StartAnim")]
        static class CaiQuanGame_StartAnim_Patch
        {
            static bool Prefix(CaiQuanGame __instance, CaiQuanType playerType, CaiQuanType npcType, ICaiQuanAction ___caiQuanAction, TaskObj ___taskObj)
            {
                if (true || !enabled)
                    return true;

                PropertyInfo property = typeof(CaiQuanGame).GetProperty("isGaming");
                property.DeclaringType.GetProperty("isGaming");
                property.SetValue(__instance, true, BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);

                ___caiQuanAction.DoPlayerCaiQuanAnim(playerType);
                ___caiQuanAction.DoNpcCaiQuanAnim(npcType);

                MethodInfo m_EndAnim = __instance.GetType().GetMethod("EndAnim", BindingFlags.NonPublic | BindingFlags.Instance);
                void m_EndAnimInvoke()
                {
                    m_EndAnim.Invoke(__instance, new object[] { });
                }
                ___taskObj = Singleton<TaskRunner>.Instance.RunDelayTask(2f/settings.RPCSpeed, false, new Action(m_EndAnimInvoke));
                return false;
            }
        }
    }
}
