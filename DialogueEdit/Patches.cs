using BehaviorDesigner.Runtime.Tasks.Basic.UnityString;
using Harmony12;
using Hont;
using Pathea;
using Pathea.GameFlagNs;
using Pathea.TipsNs;
using Pathea.UISystemNs;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DialogueEdit
{
    public partial class Main
    {
        [HarmonyPatch(typeof(TextMgr), "Get")]
        private static class TextMgr_Get_Patch
        {
            // Token: 0x06000008 RID: 8 RVA: 0x0000223C File Offset: 0x0000043C
            private static void Postfix(TextMgr __instance, int id, ref string __result)
            {
                if (!enabled)
                    return;

                if(dictStrings == null)
                {
                    return;
                }

                if (dictStrings.ContainsKey(id))
                {
                    __result = dictStrings[id];
                    __result = __result.Replace("\\r", "\r").Replace("\\n", "\n");
                }
                else if (replaceStrings.Count > 0)
                {
                    foreach (KeyValuePair<string,string> kvp in replaceStrings)
                    {
                        __result = __result.Replace(kvp.Key, kvp.Value);
                    }
                }
            }
        }
        [HarmonyPatch(typeof(TextMgr), "Load")]
        private static class TextMgr_Load_Patch
        {
            // Token: 0x06000008 RID: 8 RVA: 0x0000223C File Offset: 0x0000043C
            private static void Prefix()
            {
                if (!enabled)
                    return;
                reloadStrings();

            }
        }
    }
}
