using Harmony12;
using Pathea.CameraSystemNs;
using Pathea.UISystemNs;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace FieldOfView
{
    public class Main
    {
        public static Settings settings { get; private set; }
        public static bool enabled;
        
        private static readonly bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        { 
            if (isDebug)
                Debug.Log((pref ? typeof(Main).Namespace + " " : "") + str);
        }

        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnToggle = OnToggle;
            modEntry.OnUpdate = OnUpdate;

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
            GUILayout.Label(string.Format("Current Field of View: <b>{0:F0}</b>", settings.FieldOfView), new GUILayoutOption[0]);
            settings.FieldOfView = GUILayout.HorizontalSlider(settings.FieldOfView, 1, 180, new GUILayoutOption[0]);
            GUILayout.Space(10f);

            settings.UseScrollWheel = GUILayout.Toggle(settings.UseScrollWheel, "Use scroll wheel to change FOV", new GUILayoutOption[0]);
            GUILayout.Space(10f);

            GUILayout.Label(string.Format("Ordinary Increment: <b>{0:F0}</b>", settings.IncrementNormal), new GUILayoutOption[0]);
            settings.IncrementNormal = GUILayout.HorizontalSlider(settings.IncrementNormal, 1f, 50f, new GUILayoutOption[0]);
            GUILayout.Space(10f);

            GUILayout.Label(string.Format("Fast Increment: <b>{0:F0}</b>", settings.IncrementFast), new GUILayoutOption[0]);
            settings.IncrementFast = GUILayout.HorizontalSlider(settings.IncrementFast, 1f, 50f, new GUILayoutOption[0]);
            GUILayout.Space(10f);

            GUILayout.Space(10f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Ordinary Increment Modifier Key: ", new GUILayoutOption[0]);
            settings.ModKeyNormal = GUILayout.TextField(settings.ModKeyNormal, new GUILayoutOption[] { GUILayout.Width(100) });
            GUILayout.EndHorizontal();
            GUILayout.Space(10f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Fast Increment Modifier Key: ", new GUILayoutOption[0]);
            settings.ModKeyFast = GUILayout.TextField(settings.ModKeyFast, new GUILayoutOption[] { GUILayout.Width(100) });
            GUILayout.EndHorizontal();

            GUILayout.Space(20f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Increase FOV Key: ", new GUILayoutOption[0]);
            settings.KeyIncrease = GUILayout.TextField(settings.KeyIncrease, new GUILayoutOption[] { GUILayout.Width(100) });
            GUILayout.EndHorizontal();
            GUILayout.Space(10f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Decrease FOV Key: ", new GUILayoutOption[0]);
            settings.KeyDecrease = GUILayout.TextField(settings.KeyDecrease, new GUILayoutOption[] { GUILayout.Width(100) });
            GUILayout.EndHorizontal();
            GUILayout.Space(10f);
        }

        private static void OnUpdate(UnityModManager.ModEntry arg1, float arg2)
        {
            if (
                (settings.UseScrollWheel && Input.mouseScrollDelta.y != 0 && (CheckKeyHeld(settings.ModKeyNormal) || CheckKeyHeld(settings.ModKeyFast))) ||
                ((CheckKeyDown(settings.KeyIncrease) || CheckKeyDown(settings.KeyDecrease)) && (CheckKeyHeld(settings.ModKeyNormal, false) || CheckKeyHeld(settings.ModKeyFast, false)))
                )
            {
                float change = CheckKeyHeld(settings.ModKeyFast) ? settings.IncrementFast : settings.IncrementNormal;

                if (Input.mouseScrollDelta.y > 0)
                    settings.FieldOfView -= change;
                else if (Input.mouseScrollDelta.y < 0)
                    settings.FieldOfView += change;
                else if (CheckKeyDown(settings.KeyIncrease))
                    settings.FieldOfView += change;
                else if (CheckKeyDown(settings.KeyDecrease))
                    settings.FieldOfView -= change;

                settings.FieldOfView = Mathf.Clamp(settings.FieldOfView, 1, 180);
                //Dbgl($"scrolling {Input.mouseScrollDelta.y}; camera fov {settings.FieldOfView}");
            }
        }



        [HarmonyPatch(typeof(PlayerItemBarCtr), "AddSlotIndex")]
        static class PlayerItemBarCtr_AddSlotIndex_Patch
        {
            static bool Prefix()
            {
                return !enabled || (!CheckKeyHeld(settings.ModKeyNormal) && !CheckKeyHeld(settings.ModKeyFast));
            }
        }
        [HarmonyPatch(typeof(StandardThirdPersonCameraController), "OnCameraActiveUpdate")]
        static class StandardThirdPersonCameraController_OnCameraActiveUpdate_Patch
        {
            static void Postfix(CameraAgent cameraAgent)
            {
                if (!enabled)
                    return;
                cameraAgent.FieldOfView = settings.FieldOfView;
            }
        }
        public static bool CheckKeyDown(string value)
        {
            try
            {
                return Input.GetKeyDown(value.ToLower());
            }
            catch
            {
                return false;
            }
        }
        public static bool CheckKeyHeld(string value, bool req = true)
        {
            try
            {
                return Input.GetKey(value.ToLower());
            }
            catch
            {
                return !req;
            }
        }
    }
}
