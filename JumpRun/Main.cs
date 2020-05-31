using Harmony12;
using Pathea;
using Pathea.ACT;
using Pathea.ModuleNs;
using System;
using System.Collections.Generic;
using System.Reflection;
using Pathea.ActorNs;
using UnityEngine;
using UnityModManagerNet;
using static Pathea.ActorMotor;

namespace JumpRun
{
    public class Main
    {
        public static Settings settings { get; private set; }
        public static bool enabled;

        private static readonly bool isDebug = false;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "JumpRun " : "") + str);
        }

        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnToggle = OnToggle;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }
        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label(string.Format("Jump height multiplier: <b>{0:F1}</b>", settings.JumpHeight), new GUILayoutOption[0]);
            settings.JumpHeight = GUILayout.HorizontalSlider(settings.JumpHeight*10f, 1f, 100f, new GUILayoutOption[0]) / 10f;
            GUILayout.Space(10f);
            GUILayout.Label(string.Format("Movement speed multiplier: <b>{0:F1}</b>", settings.MovementSpeed), new GUILayoutOption[0]);
            settings.MovementSpeed = GUILayout.HorizontalSlider(settings.MovementSpeed * 10f, 10f, 100f, new GUILayoutOption[0]) / 10f;
            GUILayout.Space(10f);
            settings.multiJump = GUILayout.Toggle(settings.multiJump, "Allow multi-jump", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            settings.jumpAttack = GUILayout.Toggle(settings.jumpAttack, "Allow jump-attack", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            settings.replaceJetpack = GUILayout.Toggle(settings.replaceJetpack, "Disable jetpack in mines", new GUILayoutOption[0]);
        }
        [HarmonyPatch(typeof(Player), "Move")]
        static class Pathea_Player_Move_Patch
        {
            static void Prefix()
            {
                if (!enabled)
                    return;
                Module<Player>.Self.actor.motor.maxSpeed = 16f*settings.MovementSpeed;
                Module<Player>.Self.actor.RunSpeed = 7.2f* settings.MovementSpeed;
                Module<Player>.Self.actor.FastRunSpeed = 16f*settings.MovementSpeed;
            }
        }

        [HarmonyPatch(typeof(ActorMotor), "JumpStart")]
        static class Pathea_ActorMotor_MoveReal_Patch
        {
            static void Prefix(ActorMotor __instance, ref ActorMotor.JumpActionParamer ___jumpParamer)
            {
                if (!enabled)
                    return;
                ___jumpParamer.JumpInitSpeed = 27f * (float) Math.Sqrt(settings.JumpHeight);
            }
        }

        private static int jumpsJumped = 0;

        [HarmonyPatch(typeof(ActorMotor), "JumpStart")]
        static class Pathea_ActorMotor_JumpStart_Patch
        {
            static bool Prefix(JumpActionParamer ___jumpParamer, BoolLogic ___motorFlag, bool ___onFly, ref float ___jumpTime, ref bool ___onJump, ref Vector3 ___jumpSpeed)
            {
                if (!enabled || !settings.multiJump)
                    return true;
                
                Dbgl("Jump Start");
                
                if (!___motorFlag.Result)
                {
                    return false;
                }
                if (___onFly || ___jumpParamer == null)
                {
                    return false;
                }
                ___jumpTime = 0f;
                ___onJump = true;
                ___jumpSpeed = Vector3.up * ___jumpParamer.JumpInitSpeed * ___jumpParamer.Multiply;
                return false;
            }
        }

        [HarmonyPatch(typeof(ACTMgr), "CanDoAction", new Type[]{typeof(int)})]
        static class CanDoAction_Patch
        {
            static bool Prefix(ref bool __result, ref ACTAction[] ____acts, int index)
            {
                if (!enabled)
                    return true;

                if ((settings.multiJump && ____acts[index] is ACTJump) || (settings.jumpAttack && ____acts[index] is ACTAttack))
                {
                    Dbgl("CanDoAction Jump");
                    jumpsJumped++;
                    __result = true;
                    return false;
                }

                return true;
            }
        }
 
        [HarmonyPatch(typeof(ACTMgr), "TryAddAction", new Type[]{typeof(int)})]
        static class TryAddAction_Patch
        {
            static bool Prefix(ACTMgr __instance, ref bool __result, ref ACTAction[] ____acts, int index, List<int> ____runs)
            {
                if (!enabled || !settings.multiJump)
                    return true;

                if (index == (int)ACType.Jump)
                {

                    for (int i = ____runs.Count - 1; i >= 0; i--)
                    {
                        if ((settings.multiJump && ____acts[____runs[i]] is ACTJump) || (settings.jumpAttack && (____acts[____runs[i]] is ACTAttack)))
                        {
                            ____acts[i].Reset();
                            ____runs.Remove(i);
                            __instance.StopAll(true);
                            __result = true;
                            return false;
                        }
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(ActorMotor), "JumpUpdate")]
        static class JumpUpdate_Patch
        {
            static void Prefix(ActorMotor __instance, bool ___onJump, Actor ___actor)
            {
                if (!enabled || !settings.multiJump)
                    return;
                if (!___actor.IsAnimTag("Jump") &&!___actor.IsAnimTag("JumpEnd"))
                {
                    jumpsJumped = 0;
                }

            }
        }

        [HarmonyPatch(typeof(Player), "PutOnJetPack")]
        static class Pathea_Player_PutOnJetPack_Patch
        {
            static bool Prefix()
            {
                return (!enabled || !settings.replaceJetpack);
            }
        }

    }
}
