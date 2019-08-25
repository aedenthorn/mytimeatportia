using Harmony12;
using Pathea.FavorSystemNs;
using Pathea.MG;
using Pathea.ModuleNs;
using PatheaScript;
using PatheaScriptExt;
using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using static Harmony12.AccessTools;

namespace MarriageMod
{
    public static partial class Main
    {
        static FieldRef<ParseObj, XmlNode> mInfoByRef = FieldRefAccess<ParseObj,XmlNode>("mInfo");
        static FieldRef<TriggerChild, Trigger> mTriggerByRef = FieldRefAccess<TriggerChild,Trigger>("mTrigger");

        [HarmonyPatch(typeof(CheckMarriage), "Do", new Type[] { })]
        static class CheckMarriage_Do_Patch
        {

            static bool Prefix(CheckMarriage __instance, ref bool __result, ref int ___npcId, bool ___flag)
            {
                if (!enabled)
                    return true;

                ___npcId = StoryHelper.GetActorId(mInfoByRef(__instance), mTriggerByRef(__instance), false);

                if (___npcId == -1)
                {
                    int s = GetCurrentSpouse();
                    __result = (s != 0) == ___flag;
                    Dbgl("Checking married: " + (s!=0)+" (needed "+___flag + ")");
                }
                else
                {
                    __result = IsSpouse(___npcId) == ___flag;
                    Dbgl("Checking married to " + StoryHelper.GetActor(___npcId).ActorName + ": " + IsSpouse(___npcId) + " (needed " + ___flag+")");
                }
                return false;
            }
        }
    }
}
