using Harmony12;
using Pathea;
using Pathea.ChildrenNs;
using Pathea.ModuleNs;
using Pathea.NpcRepositoryNs;
using System;

namespace MarriageMod
{
    public static partial class Main
    {
        // set children's parent to settings spouse

        [HarmonyPatch(typeof(ChildrenModule), "UpdateBaseData", new Type[] { })]
        static class ChildrenModule_UpdateBaseData_Patch
        {
            static void Postfix(ref int ___marriageID, ref bool ___isOS)
            {
                if (!enabled)
                    return;

                ___marriageID = GetCurrentSpouse();
                ___isOS = (Module<NpcRepository>.Self.GetNpcGender(___marriageID) != Module<Player>.Self.GetGender() || Module<Player>.Self.GetGender() == Gender.Female);
            }
        }

    }

}
