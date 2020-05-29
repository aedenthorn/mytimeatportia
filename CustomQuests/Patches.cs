using Ccc;
using Harmony12;
using Pathea.Conversations;
using PatheaScript;

namespace CustomQuests
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

                if (dictStrings.ContainsKey(id))
                {
                    __result = dictStrings[id];
                    __result = __result.Replace("\\r", "\r").Replace("\\n", "\n");
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(TextMgr), "Load")]
        private static class TextMgr_Load_Patch
        {
            private static void Prefix()
            {
                if (!enabled)
                    return;
                AddStrings();

            }
        }

        [HarmonyPatch(typeof(Story), "LoadAllScriptForBaseInfo")]
        private static class Story_LoadAllScriptForBaseInfo_Patch
        {
            private static void Postfix()
            {
                if (!enabled)
                    return;
                AddScripts();

            }
        }
        [HarmonyPatch(typeof(ConvBase), "LoadData")]
        private static class ConvBase_LoadData_Patch
        {
            private static void Postfix()
            {
                if (!enabled)
                    return;
                AddConv();

            }
        }

        [HarmonyPatch(typeof(Factory), "GetScriptPath")]
        private static class Factory_GetScriptPath_Patch
        {
            private static bool Prefix(int id, ref string __result)
            {
                if (!enabled)
                    return true;
                if (newMissions.ContainsKey(id))
                {
                    Dbgl("loading " + id + " from " + newMissions[id]);
                    __result = newMissions[id];
                    return false;
                }
                return true;
            }
        }
    }
}
