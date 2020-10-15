using Harmony;
using Pathea;
using Pathea.ModuleNs;
using Pathea.PlayerAbilityNs;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace MoreSkillPoints
{
    public partial class Main
    {
        public static Settings settings { get; private set; }
        public static bool enabled;
        
        public static List<int> vacuumed;
        public static string addPoints = "";

        private static readonly bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? typeof(Main).Namespace + " " : "") + str);
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

            GUILayout.Label(string.Format("Extra Skill Points Per Level: <b>{0}</b>", settings.ExtraPoints), new GUILayoutOption[0]);
            settings.ExtraPoints = (int)GUILayout.HorizontalSlider(settings.ExtraPoints, 0f, 100f, new GUILayoutOption[0]);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Add Skill Points:", new GUILayoutOption[0]);
            addPoints = GUILayout.TextField(addPoints, new GUILayoutOption[0]);
            if (GUILayout.Button("Add", new GUILayoutOption[0]) && int.TryParse(addPoints, out int points))
            {
                Module<PlayerAbilityModule>.Self.GainPoint(points);
            }
            GUILayout.EndHorizontal();
        }


        [HarmonyPatch(typeof(Player))]
        [HarmonyPatch(nameof(Player.AddExp))]
        public static class GainPoint_Patch
        {
            static void Prefix(Player __instance, ref int __state)
            {
                __state = __instance.ActorLevel;
            }
            static void Postfix(Player __instance, int __state)
            {
                if (__state != __instance.ActorLevel)
                {
                    Module<PlayerAbilityModule>.Self.GainPoint((__instance.ActorLevel - __state)*settings.ExtraPoints);
                }
            }
        }
    }
}
