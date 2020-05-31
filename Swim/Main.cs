using Harmony12;
using static Harmony12.AccessTools;
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
using System.Collections;
using Hont;
using Pathea.CameraSystemNs;
using Pathea.InputSolutionNs;

namespace Swim
{
    public class Main
    {
        public static Settings settings { get; private set; }
        public static bool enabled;

        private static readonly bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "Swim " : "") + str);
        }

        private static int count = 0;
        private static bool startSwim = false;
        private static bool swimming = false;
        private static int animFrame = 0;

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
            settings.doSwim = GUILayout.Toggle(settings.doSwim, "Enable Swimming (turn this off to simply disable water)", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            settings.animateSwim = GUILayout.Toggle(settings.animateSwim, "Play Swim Animation", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            GUILayout.Label(string.Format("Swim Speed: <b>{0:F2}</b>", settings.swimSpeed), new GUILayoutOption[0]);
            settings.swimSpeed = GUILayout.HorizontalSlider(settings.swimSpeed * 100f, 1f, 1000f, new GUILayoutOption[0]) / 100f;
            GUILayout.Space(10f);
            GUILayout.Label(string.Format("Bob Magnitude: <b>{0:F2}</b>", settings.bobMagnitude), new GUILayoutOption[0]);
            settings.bobMagnitude = GUILayout.HorizontalSlider(settings.bobMagnitude * 100f, 0f, 100f, new GUILayoutOption[0]) / 100f;
            GUILayout.Space(10f);
            GUILayout.Label(string.Format("Bob Speed: <b>{0:F2}</b>", settings.bobSpeed), new GUILayoutOption[0]);
            settings.bobSpeed = GUILayout.HorizontalSlider(settings.bobSpeed * 100f, 1f, 200f, new GUILayoutOption[0]) / 100f;
        }

        [HarmonyPatch(typeof(Player), "BeginCorrect")]
        static class Player_BeginCorrect_Patch
        {
            static bool Prefix()
            {
                if (!enabled)
                    return true;

                if (!settings.doSwim)
                    return false;
                if (!swimming)
                {
                    count = 0;
                    swimming = true;
                    Singleton<TaskRunner>.Self.StartCoroutine(Swim());
                }
                startSwim = true;

                return false;
            }
        }

        private static IEnumerator Swim()
        {
            float seconds = Time.fixedTime;
            while (Time.fixedTime - seconds < 1f)
            {
                if (!enabled)
                    yield break;
                if (settings.animateSwim && animFrame++ > 50)
                {
                    animFrame = 0;
                    typeof(ActorMotor).GetMethod("PlayAnimation", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(string), typeof(float) }, new ParameterModifier[0]).Invoke(Player.Self.actor.motor, new object[] { "FlyStart", 0.1f });
                }
                if (Time.fixedTime - seconds > 0.1f)
                {
                    if (startSwim)
                    {
                        startSwim = false;
                        seconds = Time.fixedTime; 
                    }
                }

                Vector3 pos = Player.Self.GamePos;

                float height = 25f;
                if (pos.x > 665f && pos.z > -185f && pos.x < 866f && pos.z < -100f) // wf river one S
                {
                    height = 85f;
                }
                else if (pos.x > 665f && pos.z > -100f && pos.x < 866f && pos.z < -68f) // wf river one N
                {
                    height = 85.5f;
                }
                else if (pos.x > 665f && pos.z > -68f && pos.x < 866f && pos.z < 61f) // wf river one N
                {
                    height = 86f;
                }
                else if (pos.x > 866f && pos.z < 252f && pos.z > -47f) // wf river 2
                {
                    height = 127f;
                }
                else if (pos.x > 905f && pos.z < -398f && pos.z > -554f) // desert
                {
                    height = 73f;
                }
                else if (pos.x < -288f && pos.z > -242f && pos.z < 12f && pos.x > -590f) // balloon
                {
                    height = 58f;
                }
                else if (pos.x < -705f && pos.z > -314f && pos.z < -127f) // western plateau
                {
                    height = 69f;
                }
                else if (pos.x > 314f && pos.x < 448f && pos.z > 10f && pos.z < 167f) //wasteland
                {
                    height = 25f;
                }
                else if (pos.z > 179f) // north
                {
                    height = 73f;
                }
                    //pos.y = 25f + (float)Math.Sin(height / 10f) / 5f;
                
                float bob = (float)Math.Sin(settings.bobSpeed * count / 10f) / 3f * settings.bobMagnitude;
                Vector3 newPos = new Vector3(0, height + bob - pos.y, 0);

                if (Module<PlayerActionModule>.Self.IsAcionPressed(ActionType.ActionMoveForward) || Module<PlayerActionModule>.Self.IsAcionPressed(ActionType.ActionMoveLeft) || Module<PlayerActionModule>.Self.IsAcionPressed(ActionType.ActionMoveRight) || Module<PlayerActionModule>.Self.IsAcionPressed(ActionType.ActionMoveBack))
                {
                    Vector3 forward = Player.Self.actor.transform.forward;
                    forward.x *= 0.2f * settings.swimSpeed;
                    forward.y = 0f;
                    forward.z *= 0.2f * settings.swimSpeed;
                    newPos += forward;
                }

                Vector3 oldDelta = FieldRefAccess<ActorMotor, Vector3>(Player.Self.actor.motor, "moveDeltaPos");
                oldDelta.y = 0;
                //newPos += oldDelta;
                //Player.Self.GamePos = pos;
                Player.Self.actor.motor.MoveByDeltaPos(newPos);
                //CameraManager.Instance.ExecuteUpdate();

                count++;
                yield return new WaitForEndOfFrame();
            }
            swimming = false;
            Dbgl("stopped swimming");
            yield break;
        }

        private class VectorRange : IVector3Range
        {
            public bool Contain(Vector3 v3)
            {
                return true;
            }
        }
    }
}
