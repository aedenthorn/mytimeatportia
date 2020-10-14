using Harmony12;
using Pathea.ConfigNs;
using Pathea.Missions;
using Pathea.ModuleNs;
using Pathea.PlayerMissionNs;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityModManagerNet;
using Mission = Pathea.PlayerMissionNs.Mission;

namespace MoreDisplayedMissions
{
    public partial class Main
    {
        public static Settings settings { get; private set; }
        public static bool enabled;
        
        public static List<int> vacuumed;

        private static readonly bool isDebug = true;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? "MoreDisplayedMissions " : "") + str);
        }

        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            typeof(OtherConfig).GetField("playerMissionMaxCount", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(OtherConfig.Self, settings.MaxMissions);
            Dbgl($"Set playerMissionMaxCount to {OtherConfig.Self.PlayerMissionMaxCount} ({settings.MaxMissions})");

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

            GUILayout.Label(string.Format("Maximum Displayed Missions (requires game restart): <b>{0}</b>", settings.MaxMissions), new GUILayoutOption[0]);
            settings.MaxMissions = (int)GUILayout.HorizontalSlider(settings.MaxMissions, 1f, 30f, new GUILayoutOption[0]);
        }


        [HarmonyPatch(typeof(MissionManager))]
        [HarmonyPatch("ToggleTrace")]
        public static class ToggleTrace_Patch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldc_I4_3)
                    {
                        codes[i].opcode = OpCodes.Ldc_I4;
                        codes[i].operand = settings.MaxMissions;

                    }
                }
                return codes.AsEnumerable();
            }
        }
    }
}
