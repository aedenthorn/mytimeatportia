using Harmony12;
using Pathea.ActorNs;
using Pathea.UISystemNs;

namespace PennyComeBack
{
    public static partial class Main
    {
        [HarmonyPatch(typeof(ActorInfo), "miniHeadIcon", MethodType.Getter)]
        static class UIUtils_GetSpriteByPath_Patch
        {
            static void Prefix(ref string ___miniHeadIcon_db)
            {
                if (!enabled)
                    return;
                if (___miniHeadIcon_db == "MiniHead/nuliu")
                    ___miniHeadIcon_db = "MiniHead/nvliu";
                //Dbgl("sprite path: "+path);
            }
        }
    }
}