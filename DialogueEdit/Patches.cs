using Harmony12;
using System.Collections.Generic;

namespace DialogueEdit
{
    public partial class Main
    {
        [HarmonyPatch(typeof(TextMgr), "Get")]
        private static class TextMgr_Get_Patch
        {
            private static bool Prefix(TextMgr __instance, int id, ref string __result)
            {
                if (!enabled || dictStrings == null)
                    return true;
                if (dictStrings.TryGetValue(id, out var str))
                {
                    __result = str.Replace("\\r", "\r").Replace("\\n", "\n");
                    return false;
                }
                return true;
            }
            private static void Postfix(TextMgr __instance, int id, ref string __result)
            {
                if (!enabled || dictStrings == null)
                    return;

                if (replaceStrings.Count > 0)
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
            private static void Prefix()
            {
                if (!enabled)
                    return;
                reloadStrings();

            }
        }
    }
}
