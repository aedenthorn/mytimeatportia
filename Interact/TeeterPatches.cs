using Harmony12;
using Pathea;
using Pathea.ActorNs;
using Pathea.ConfigNs;
using Pathea.EngagementsNs.GameNs;
using Pathea.FavorSystemNs;
using Pathea.MiniGameNs;
using Pathea.ModuleNs;
using Pathea.UISystemNs;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Interact
{
    public partial class Main
    {
        public static float startNotInputTime = 2f;
        private static readonly Dictionary<string, string> InteractDlg = new Dictionary<string, string>()
        {
            { "Hug", "Hug me!" },
            { "Interact_Kiss", "Kiss me!" },
            { "Interact_Massage", "Let me massage you!" }
        };

        public static readonly string[] LikeLists = 
        {
            "I like this!",
            "This is nice!",
            "Wonderful!",
            "You're good at this!",
            "Yes!",
            "I'm so happy!",
            "This is great!"
        };
        public static readonly string[] DislikeLists =
        {
            "I don't like this...",
            "This isn't right...",
            "Terrible...",
            "You're not very good at this...",
            "No...",
            "I'm not happy...",
            "This isn't great..."
        };

        public static bool CanTeeterInteract(Actor actor)
        {
            if (actor == null)
                return false;
            int relationship = FavorRelationshipUtil.GetRelationShip(actor.InstanceId);
            return ((settings.HugRelationshipMin == -1 || relationship >= settings.HugRelationshipMin) &&
                    (settings.KissRelationshipMin == -1 || relationship >= settings.KissRelationshipMin) &&
                    (settings.MassageRelationshipMin == -1 || relationship >= settings.MassageRelationshipMin));

        }

        public static Actor lastTeeterActor;

        [HarmonyPatch(typeof(TeeterGameRunner), "StartGame")]
        static class TeeterGameRunner_StartGame_Patch
        {
            static void Postfix(TeeterGameRunner __instance, BoolFalse ___npcHide, BoolFalse ___hidePlayer)
            {
                Actor npcActor = (Actor)__instance.GetType().BaseType.GetProperty("NpcActor").GetValue(__instance, null);
                if (!enabled || !settings.ReplaceTeeterTotterGame || !CanTeeterInteract(npcActor))
                    return;
                Module<Player>.Self.actor.RemoveVisible(___hidePlayer);
                npcActor.RemoveVisible(___npcHide);
            }
        }

        [HarmonyPatch(typeof(TeeterGameCtr), "StartGame")]
        static class TeeterGameCtr_StartGame_Patch
        {
            static void Postfix(Actor npcActor, TeeterGameCtr __instance, ref bool ___playing, ref bool ___waitForInput,
                ref float ___startInputTime, ref float ___curTime, TeeterActorCtr ___player, TeeterActorCtr ___npc, ref WhiteCat.Tween.Tweener ___teeterTW, ref int ___gameScores, ref int ___teeterDir)
            {
                lastTeeterActor = npcActor;
                if (!enabled || !settings.ReplaceTeeterTotterGame || !CanTeeterInteract(npcActor))
                    return;
                teeterGameRunning = true;
                ___teeterTW.enabled = false;
                //___teeterDir = 0;
                Transform t1 = AccessTools.FieldRefAccess<TeeterActorCtr, GameObject>(___player, "tempmodel").transform;
                Transform t2 = AccessTools.FieldRefAccess<TeeterActorCtr, GameObject>(___npc, "tempmodel").transform;

                Vector3 midpoint = (t1.position + t2.position) / 2;

                Player.Self.actor.gamePos = new Vector3(214.5f, 48.4f, -68.5f);
                lastTeeterActor.gamePos = new Vector3(216.0f, 48.4f, -69.8f);

                for (int i = 0;
                    i < AccessTools.FieldRefAccess<TeeterActorCtr, GameObject>(___player, "tempmodel")
                        .GetComponentsInChildren<Renderer>(true).Length;
                    i++)
                {
                    AccessTools.FieldRefAccess<TeeterActorCtr, GameObject>(___player, "tempmodel").GetComponentsInChildren<Renderer>(true)[i].enabled = false;
                }
                for (int i = 0;
                    i < AccessTools.FieldRefAccess<TeeterActorCtr, GameObject>(___npc, "tempmodel")
                        .GetComponentsInChildren<Renderer>(true).Length;
                    i++)
                {
                    AccessTools.FieldRefAccess<TeeterActorCtr, GameObject>(___npc, "tempmodel").GetComponentsInChildren<Renderer>(true)[i].enabled = false;
                }
            }
        }

        [HarmonyPatch(typeof(TeeterGameCtr), "Update")]
        static class TeeterGameCtr_Update_Patch
        {
            static bool Prefix(TeeterGameCtr __instance, bool ___playing, ref bool ___waitForInput, ref float ___startInputTime, ref float ___curTime, ref TeeterActorCtr ___player, ref TeeterActorCtr ___npc)
            {
                if (!enabled || !settings.ReplaceTeeterTotterGame || !CanTeeterInteract(lastTeeterActor))
                    return true;

                if (___playing)
                {
                    __instance.StopJump(null);
                    if (___waitForInput)
                    {
                        MethodInfo dynMethod = __instance.GetType().GetMethod("ScoreChange", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (Input.GetKeyDown(settings.KissKey))
                        {
                            lastAnim = 1;
                            DoInteract(Player.Self.actor, lastTeeterActor, lastAnim);
                            dynMethod.Invoke(__instance, new object[] { 0 });
                        }
                        else if (Input.GetKeyDown(settings.HugKey))
                        {
                            lastAnim = 0;
                            DoInteract(Player.Self.actor, lastTeeterActor, lastAnim);
                            dynMethod.Invoke(__instance, new object[] { 0 });
                        }
                        else if (Input.GetKeyDown(settings.MassageKey))
                        {
                            lastAnim = 2;
                            DoInteract(Player.Self.actor, lastTeeterActor, lastAnim);
                            dynMethod.Invoke(__instance, new object[] { 0 });
                        }
                        ___startInputTime += Time.deltaTime;
                        if (___startInputTime >= OtherConfig.Self.TeetGameWaitTime)
                        {
                            lastAnim = 999;
                            dynMethod.Invoke(__instance, new object[] { 0 });
                            ___waitForInput = false;
                        }
                    }
                    else
                    {
                        ___startInputTime = 0f;
                        if (!isInteracting && __instance.OnActorSpeak != null)
                        {
                            System.Random rand = new System.Random();
                            int idx = rand.Next(0, 3);
                            rightAnim = idx;
                            __instance.OnActorSpeak(false, InteractDlg[Interacts[idx]]);
                            startNotInputTime = 0f;
                            ___waitForInput = true;
                        }
                    }
                    ___curTime += Time.deltaTime;
                    if (___curTime >= OtherConfig.Self.TeetGameTime)
                    {
                        ___playing = false;
                        teeterGameRunning = false;
                        lastTeeterActor = null;
                        MethodInfo dynMethod2 = __instance.GetType().GetMethod("EndGame", BindingFlags.NonPublic | BindingFlags.Instance);
                        dynMethod2.Invoke(__instance, new object[] { 0 });
                    }
                }

                return false;
            }
        }

        public static bool teeterGameRunning = false;
        public static int lastAnim = 0;
        public static int rightAnim = -1;

        [HarmonyPatch(typeof(TeeterGameCtr), "ScoreChange")]
        static class TeeterGameCtr_ScoreChange_Patch
        {
            static bool Prefix(TeeterGameCtr __instance, ref int ___gameScores, ref bool ___waitForInput)
            {
                if (!enabled || !settings.ReplaceTeeterTotterGame || !CanTeeterInteract(lastTeeterActor))
                    return true;
                int num = ___gameScores;

                if (lastAnim == rightAnim)
                {
                    num = Mathf.Clamp(num + 1, -4, 4);
                    __instance.OnActorSpeak(false, LikeLists[new System.Random().Next(0,LikeLists.Length)]);
                }
                else
                {
                    num = Mathf.Clamp(num - 1, -4, 4);
                    __instance.OnActorSpeak(false, DislikeLists[new System.Random().Next(0, DislikeLists.Length)]);
                }
                ___gameScores = num;
                ___waitForInput = false;
                return false;
            }
        }
        [HarmonyPatch(typeof(TeeterUI), "Update")]
        static class TeeterUI_Update_Patch
        {
            static void Postfix(ref RectTransform ___heightStepPointer, ref GameObject ___controlHint)
            {
                if (!enabled || !settings.ReplaceTeeterTotterGame || !CanTeeterInteract(lastTeeterActor))
                    return;

                ___heightStepPointer.gameObject.SetActive(false);
                ___controlHint.SetActive(false);
            }
        }
        [HarmonyPatch(typeof(TeeterUI), "OnPlayerInput")]
        static class TeeterUI_Patch
        {
            static bool Prefix()
            {
                if (!enabled || !settings.ReplaceTeeterTotterGame || !CanTeeterInteract(lastTeeterActor))
                    return true;

                return false;
            }
        }
    }
}
