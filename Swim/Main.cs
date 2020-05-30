using Harmony12;
using static Harmony.AccessTools;
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

        private static int height = 0;
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
            GUILayout.Label(string.Format("Bob Magnitude: <b>{0:F2}</b>", settings.bobMagnitude), new GUILayoutOption[0]);
            settings.bobMagnitude = GUILayout.HorizontalSlider(settings.bobMagnitude * 100f, 0f, 100f, new GUILayoutOption[0]) / 100f;
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
                //pos.y = 25f + (float)Math.Sin(height / 10f) / 5f;
                float bob = (float)Math.Sin(height / 10f) / 3f * settings.bobMagnitude;
                Vector3 newPos = new Vector3(0, 25f + bob - pos.y, 0);

                if (Module<PlayerActionModule>.Self.IsAcionPressed(ActionType.ActionMoveForward) || Module<PlayerActionModule>.Self.IsAcionPressed(ActionType.ActionMoveLeft) || Module<PlayerActionModule>.Self.IsAcionPressed(ActionType.ActionMoveRight) || Module<PlayerActionModule>.Self.IsAcionPressed(ActionType.ActionMoveBack))
                {
                    Vector3 forward = Player.Self.actor.transform.forward;
                    forward.x *= 0.1f;
                    forward.y = 0f;
                    forward.z *= 0.1f;
                    newPos += forward;
                }

                Vector3 oldDelta = FieldRefAccess<ActorMotor, Vector3>(Player.Self.actor.motor, "moveDeltaPos");
                oldDelta.y = 0;
                //newPos += oldDelta;
                //Player.Self.GamePos = pos;
                Player.Self.actor.motor.MoveByDeltaPos(newPos);
                //CameraManager.Instance.ExecuteUpdate();

                height++;
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
